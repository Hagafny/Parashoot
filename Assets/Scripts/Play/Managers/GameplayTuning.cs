using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SINGLE SOURCE OF TRUTH for gameplay pacing values.
///
/// WHY THIS FILE EXISTS
/// --------------------
/// This project is saved with BINARY serialization (ProjectSettings/EditorSettings.asset),
/// so Play.unity and every prefab are binary blobs. A serialized value on the Cow prefab
/// OVERRIDES the C# field initializer in CowStats.cs — meaning editing `fireDelay = 0.75f`
/// in CowStats.cs changes nothing at runtime. The only reliable way to retune without
/// hand-editing binary assets in the Unity Editor is to ASSIGN the values at runtime.
///
/// So: every pacing number lives here, and the systems below pull from it:
///   CowStats.Awake()        -> movement / rotation / fire delay / invincibility
///   BulletMovement.Start()  -> bullet speed
///   BaloonSpawner.Start()   -> balloon cadence
///   PowerUpSpawner.Start()  -> crate cadence
///
/// TO ITERATE ON FEEL: change a number here, re-enter Play mode. Nothing else to touch.
///
///
/// THE DESIGN PROBLEM THIS SOLVES
/// ------------------------------
/// The cows are locked to their spawn columns (HumanMovement clamps x), so EVERY shot
/// travels the same distance: the full arena width. That makes bullet travel time a single
/// well-defined constant, and it exposes the ratio that was driving the frantic feel:
///
///     shotsInFlight = bulletTravelTime / fireDelay
///
/// BEFORE: 24 u/s across a ~28u arena = ~1.17s travel, against a 0.75s fire delay
///         -> ratio 1.56. You could ALWAYS fire again before your previous shot resolved.
///         Spamming carried zero cost, ~3+ bullets were live at once, and a miss was
///         free because the next shot was already on its way. Hence Shoot->Shoot->Shoot.
///
/// AFTER:  18 u/s = ~1.56s travel, against a 1.65s fire delay
///         -> ratio ~0.94. You fire, and you MUST watch it resolve before firing again.
///         A miss now costs you the whole exchange. Screen density falls out of this
///         structurally (max ~1 bullet per cow in flight) rather than being capped by fiat.
///
/// KEEP THE RATIO <= 1.0 WHEN RETUNING. Raising BulletSpeed shortens travel time, so FireDelay
/// has to come down with it or the gap between "your shot landed" and "you may fire again"
/// becomes dead air the player just stands through.
///
/// That single inversion — fireDelay >= travelTime — is what converts shooting from a
/// spam action into a commitment. Everything else below supports it.
/// </summary>
public static class GameplayTuning
{
    // ---------------------------------------------------------------------
    // SHOOTING CADENCE  (objective 1 + 6: commitment, a miss should matter)
    // ---------------------------------------------------------------------

    /// Seconds between shots. Held just under BulletTravelTime so your shot resolves right as the
    /// gun comes back up — commitment without dead air. This is the keystone value of the pass.
    /// Lowered from 1.9 alongside the BulletSpeed increase to hold that relationship.
    public const float FireDelay = 1.65f;

    // ---------------------------------------------------------------------
    // BULLET TRAVEL  (objective 2: threatening but readable)
    // ---------------------------------------------------------------------

    /// Units/sec. Down from the original 24, up from 15. At ~28u of arena this is ~1.56s of
    /// travel — still long enough to see where a bullet is headed, whether it passed through a
    /// balloon, and decide to dodge, but with more menace behind it than 15 had.
    public const float BulletSpeed = 18f;

    // ---------------------------------------------------------------------
    // MOVEMENT  (objective 4: planning over twitch, positioning as an advantage)
    // ---------------------------------------------------------------------

    /// Units/sec. Down from the original 12, up from 8.5. Full arena traverse (21.2u) is ~2.3s,
    /// so where you are standing is still a commitment rather than something you can undo
    /// instantly — but crossing the arena no longer feels like wading.
    public const float MovementSpeed = 9.2f;

    /// Seconds to ramp from rest to full speed. Previously movement was instantaneous — the
    /// cow snapped to full velocity on keypress, which is what made dodging a twitch reflex.
    /// A ramp means changing direction COSTS you time, so you have to predict rather than react.
    public const float AccelerationTime = 0.22f;

    /// Seconds to ramp back to rest. Shorter than acceleration so the cow still feels
    /// responsive to a stop input and never feels like it's sliding on ice.
    public const float DecelerationTime = 0.14f;

    /// Effective aim rate is (RotationSpeed * 15) degrees/sec — 72 deg/s here, down from the
    /// original 150 but up from 54. Sweeping the full 90-degree arc takes ~1.25s instead of
    /// ~1.7s: still too slow to snap onto a moving target, so leading a shot still matters,
    /// but tracking no longer feels like turning a crank.
    public const float RotationSpeed = 4.8f;

    // ---------------------------------------------------------------------
    // RECOVERY
    // ---------------------------------------------------------------------

    /// Post-hit invincibility. Up from 0.2s. With only ~1 bullet in flight per cow the old
    /// window's job (preventing same-frame multi-hits) is already handled by cadence, so this
    /// is now a genuine beat to reposition after being hit instead of an anti-glitch guard.
    public const float InvincibilitySeconds = 0.35f;

    // ---------------------------------------------------------------------
    // BALLOON / CRATE TRAVEL
    // ---------------------------------------------------------------------
    //
    // Balloon rise and crate fall are driven by Rigidbody2D.gravityScale authored on the BINARY
    // prefabs, which cannot be read from source. So rather than replace that motion with a flat
    // velocity (which would discard whatever the prefab intended), these SCALE it: the authored
    // acceleration curve is preserved, just faster.
    //
    // Because the base value is unknown, these are relative. There's no absolute number to reason
    // against from source, so tune by feel.
    //
    // IMPORTANT — THESE ARE NOT LINEAR. The motion is accelerating, so travel speed goes with the
    // SQUARE ROOT of the multiplier (v = sqrt(2*a*d)). To halve how fast a balloon crosses, use a
    // QUARTER of the multiplier, not half. 0.25 below is therefore "half the prefab's speed".
    //
    // Both stay accelerating rather than constant-speed, so they are slowest at their spawn edge
    // and fastest as they leave the far side. Scaling lowers the whole curve but keeps that shape,
    // which is why they can still feel like they whip past at the end.

    /// Multiplier on the Balloon prefab's authored gravityScale (negative, hence the rise).
    /// 0.58 => ~76% of the prefab's crossing speed: quick enough to feel buoyant, still under
    /// bullet speed so the bullet stays the fastest thing on screen.
    public const float BalloonRiseMultiplier = 0.58f;

    /// Multiplier on the crate prefab's authored gravityScale (positive, hence the fall).
    /// 0.42 => ~65% of prefab speed. Kept below the balloon: it's on a parachute, and a steadier
    /// descent is what gives both players time to decide whether to contest it.
    public const float CrateFallMultiplier = 0.42f;

    /// How long a balloon or crate stays alive — and shootable — after fully clearing the view.
    /// Not zero on purpose: leading one and hitting it just after it leaves the screen is a
    /// legitimate play. See OffscreenLifetime, which measures this instead of guessing a lifetime
    /// from speed (speed isn't knowable from source, and changes whenever the multipliers do).
    public const float OffscreenGraceSeconds = 2.5f;

    // ---------------------------------------------------------------------
    // SCREEN DENSITY  (objective 5: understand the battlefield at a glance)
    // ---------------------------------------------------------------------

    /// Seconds between balloons. Balloons are OPPORTUNITIES — the point of spacing them out is
    /// that an engagement forms around one, instead of three overlapping and creating noise.
    /// Tightened from 10 so opportunities arrive more often without stacking up.
    public const float BalloonInterval = 8.5f;

    /// Seconds between power-up crates. Tightened from 18, and deliberately not a multiple of
    /// BalloonInterval so crates and balloons drift in and out of phase rather than
    /// arriving together every time.
    public const float PowerUpInterval = 15f;

    // ---------------------------------------------------------------------
    // AI DIFFICULTY  (multiplicative — see GameManager.SetAiLevel)
    // ---------------------------------------------------------------------
    //
    // These MUST be multipliers, not the additive offsets the game used to use. The old code
    // did `movementSpeed -= 7`, which with the new base of 7 would produce a completely
    // frozen AIEasy cow and a NEGATIVE rotation speed. Multipliers scale correctly with
    // whatever base values are set above, so this file stays safe to iterate on.

    public struct AiScale
    {
        public float Movement;
        public float Rotation;
        public float FireDelay; // >1 = shoots less often

        public AiScale(float movement, float rotation, float fireDelay)
        {
            Movement = movement;
            Rotation = rotation;
            FireDelay = fireDelay;
        }
    }

    public static AiScale ScaleFor(CowType type)
    {
        switch (type)
        {
            case CowType.AIEasy:   return new AiScale(0.55f, 0.55f, 1.60f);
            case CowType.AINormal: return new AiScale(0.85f, 0.85f, 1.25f);
            case CowType.AIHard:   return new AiScale(1.15f, 1.15f, 0.85f);
            case CowType.AIInsane: return new AiScale(1.35f, 1.35f, 0.70f);
            default:               return new AiScale(1f, 1f, 1f);
        }
    }

    // ---------------------------------------------------------------------
    // DIAGNOSTICS
    // ---------------------------------------------------------------------

    /// Set true to log the MEASURED arena width and the resulting pacing ratios on round
    /// start. The arena width lives in the binary scene and cannot be read from source, so
    /// this is how you confirm the shots-in-flight ratio above matches reality.
    /// (static readonly rather than const so toggling it off doesn't make the guard in
    /// PacingDiagnostics.Bootstrap compile to unreachable code.)
    public static readonly bool LogPacingDiagnostics = true;
}

/// <summary>
/// Measures the real arena at runtime and logs the pacing ratios that result from
/// GameplayTuning. Self-bootstrapping, same pattern as SlowMotionDirector / MenuNavigation —
/// no per-scene setup, which matters because the scenes are binary and can't be wired by hand
/// outside the Editor.
///
/// This exists because arena width is the one input to the tuning math that is NOT readable
/// from source. Rather than guess, this prints the true number so the values can be dialled in.
/// </summary>
public class PacingDiagnostics : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!GameplayTuning.LogPacingDiagnostics)
            return;

        var go = new GameObject("PacingDiagnostics");
        DontDestroyOnLoad(go);
        go.AddComponent<PacingDiagnostics>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        StartCoroutine(ReportWhenCowsExist());
    }

    private System.Collections.IEnumerator ReportWhenCowsExist()
    {
        // The cows are spawned by GameManager.Start, so wait for them rather than assuming
        // script execution order.
        GameObject cow1 = null;
        GameObject cow2 = null;

        for (int frame = 0; frame < 300; frame++)
        {
            cow1 = GameObject.Find("Cow1");
            cow2 = GameObject.Find("Cow2");
            if (cow1 != null && cow2 != null)
                break;
            yield return null;
        }

        if (cow1 == null || cow2 == null)
            yield break; // Not the Play scene.

        CowStats stats = cow1.GetComponent<CowStats>();
        if (stats == null)
            yield break;

        float arenaWidth = Mathf.Abs(cow1.transform.position.x - cow2.transform.position.x);
        float travelTime = arenaWidth / GameplayTuning.BulletSpeed;
        float shotsInFlight = travelTime / stats.fireDelay;
        float verticalRange = stats.yMaxBounadry - stats.yMinBounadry;
        float traverseTime = verticalRange / stats.movementSpeed;

        Debug.LogFormat(
            "[Pacing] arena width {0:0.0}u | bullet {1:0.0} u/s -> travel {2:0.00}s | fireDelay {3:0.00}s\n" +
            "[Pacing] shots-in-flight ratio {4:0.00} (want <= 1.0 so every shot resolves before the next)\n" +
            "[Pacing] vertical range {5:0.0}u | move {6:0.0} u/s -> full traverse {7:0.00}s\n" +
            "[Pacing] dodge window: {8:0.0}u of travel available during one bullet's flight",
            arenaWidth,
            GameplayTuning.BulletSpeed, travelTime, stats.fireDelay,
            shotsInFlight,
            verticalRange, stats.movementSpeed, traverseTime,
            stats.movementSpeed * travelTime);

        if (shotsInFlight > 1f)
        {
            Debug.LogWarningFormat(
                "[Pacing] shots-in-flight is {0:0.00} (>1): a cow can fire again before its previous " +
                "shot lands, which is the spam loop this tuning pass targets. Raise " +
                "GameplayTuning.FireDelay to at least {1:0.00}s, or raise BulletSpeed.",
                shotsInFlight, travelTime);
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Triggers a brief cinematic slow-motion ~0.2s before a GUARANTEED KILLING hit lands on a player.
///
/// Self-bootstrapping (no per-scene setup, like MenuNavigation): each frame it scans every active
/// bullet, casts a short distance ahead along the bullet's travel path, and if the first thing that
/// bullet would hit is a player whose next unblocked hit ends the round, it slows time — holding
/// through the impact so you watch the kill connect, then easing back to full speed.
///
/// Only meaningful in the Play scene; elsewhere there are no "Bullet"-tagged objects, so it no-ops.
/// A scene change always restores Time.timeScale so the game can never get stuck in slow motion.
/// </summary>
public class SlowMotionDirector : MonoBehaviour
{
    private const float LeadTime = 0.2f;          // How long before the predicted impact to trigger.
    private const float CastRadius = 0.3f;        // Forgiveness around the bullet's path when predicting.
    private const float SlowScale = 0.12f;        // Time.timeScale during the slow-mo.
    private const float MaxHoldSeconds = 1.0f;    // Real-time safety cap on the slow hold (in case the shot is intercepted and the kill never lands).
    private const float PostImpactSeconds = 0.25f;// Real-time beat to linger after the kill lands.
    private const float EaseBackSeconds = 0.4f;   // Real-time ramp from slow speed back to normal.

    private float baseFixedDelta;
    private bool slowMotionActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("SlowMotionDirector");
        DontDestroyOnLoad(go);
        go.AddComponent<SlowMotionDirector>();
    }

    private void Awake()
    {
        baseFixedDelta = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RestoreTime();
    }

    // A scene change (round restart, exit to menu) must never leave the game stuck in slow motion.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        slowMotionActive = false;
        RestoreTime();
    }

    private void Update()
    {
        if (slowMotionActive)
            return;

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        for (int i = 0; i < bullets.Length; i++)
        {
            CowHealth victim = LethalVictim(bullets[i]);
            if (victim != null)
            {
                StartCoroutine(SlowMotionRoutine(victim));
                break;
            }
        }
    }

    /// <summary>
    /// If this bullet is about to score an unobstructed killing hit within LeadTime, returns the
    /// victim's CowHealth; otherwise null.
    /// </summary>
    private CowHealth LethalVictim(GameObject bullet)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb == null)
            return null;

        Vector2 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        if (speed < 0.01f)
            return null; // Not moving yet (velocity is applied in the bullet's Start).

        Vector2 origin = rb.position;
        Vector2 direction = velocity / speed;
        float distance = speed * LeadTime;

        // The FIRST collider along the path decides: if anything (another bullet, a shield, a
        // power-up box) is in the way, the hit isn't guaranteed, so only a direct player hit counts.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, CastRadius, direction, distance);
        Collider2D firstCollider = null;
        float nearest = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.gameObject == bullet)
                continue; // Ignore the bullet's own collider.
            if (hits[i].distance <= 0.02f)
                continue; // Overlapping the start point (e.g. the shooter at the muzzle) — not an incoming hit.
            if (hits[i].distance < nearest)
            {
                nearest = hits[i].distance;
                firstCollider = col;
            }
        }

        if (firstCollider == null || !firstCollider.CompareTag("Player"))
            return null;

        CowHealth health = firstCollider.GetComponent<CowHealth>();
        return (health != null && health.WouldNextHitBeFatal()) ? health : null;
    }

    private IEnumerator SlowMotionRoutine(CowHealth target)
    {
        slowMotionActive = true;
        Time.timeScale = SlowScale;
        Time.fixedDeltaTime = baseFixedDelta * SlowScale;

        // Freeze the doomed cow so it cannot dodge out of the guaranteed kill. Both HumanMovement
        // and AIMovement drive the cow by setting position directly, so disabling the Movement
        // component fully locks it in place.
        Movement victimMovement = target != null ? target.GetComponent<Movement>() : null;
        if (victimMovement != null)
            victimMovement.enabled = false;

        // Hold the slow-mo until the fatal hit actually lands (target dies) or a safety timeout,
        // so the player watches the killing blow connect in slow motion.
        float elapsed = 0f;
        while (elapsed < MaxHoldSeconds && (target == null || !target.m_Dead))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Linger a beat on the impact.
        float beat = 0f;
        while (beat < PostImpactSeconds)
        {
            beat += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ramp back to full speed.
        float eased = 0f;
        while (eased < EaseBackSeconds)
        {
            eased += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(eased / EaseBackSeconds);
            Time.timeScale = Mathf.Lerp(SlowScale, 1f, f);
            Time.fixedDeltaTime = baseFixedDelta * Time.timeScale;
            yield return null;
        }

        RestoreTime();

        // If the kill never landed (shot intercepted), unfreeze the cow so play can continue.
        // If it died, GameManager's end-of-round logic already owns its control state.
        if (victimMovement != null && target != null && !target.m_Dead)
            victimMovement.enabled = true;

        slowMotionActive = false;
    }

    private void RestoreTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDelta;
    }
}

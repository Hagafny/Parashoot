using UnityEngine;

/// <summary>
/// Destroys an object a short grace period AFTER it has left the screen, rather than on a fixed
/// timer.
///
/// WHY NOT A FIXED LIFETIME
/// ------------------------
/// Balloons and crates move under Rigidbody gravity authored on binary prefabs, so their real
/// speed isn't knowable from source and changes every time the multipliers in GameplayTuning are
/// retuned. Any hardcoded "seconds to live" is therefore wrong the moment speed changes — too
/// short and the object vanishes in mid-screen (which is exactly what happened when the rise was
/// slowed down), too long and dead objects linger.
///
/// Watching the actual position removes the coupling entirely: however fast or slow the object
/// ends up travelling, it lives exactly until it clears the view plus GraceSeconds. Retuning
/// speed can never again strand or orphan one.
///
/// THE GRACE PERIOD
/// ----------------
/// Deliberately not zero. Leading a balloon and shooting it just after it leaves the top of the
/// screen is a legitimate play, so the object has to stay alive and shootable for a beat once
/// it's out of sight — it just shouldn't hang around indefinitely.
/// </summary>
public class OffscreenLifetime : MonoBehaviour
{
    /// How long to stay alive (and shootable) after fully clearing the view.
    public float GraceSeconds = 2.5f;

    /// Extra world units past the camera edge before counting as "gone", so an object isn't
    /// considered offscreen while part of its sprite is still visible.
    public float EdgeMargin = 2.5f;

    /// Backstop for anything that never becomes visible at all (e.g. a popped balloon that falls
    /// straight back out the way it came). Real-time seconds.
    public float MaxLifetimeSeconds = 40f;

    // Balloons spawn at y = -19 and crates at y = +19 — i.e. already offscreen. Without this the
    // grace timer would start immediately and destroy them before they ever came into view.
    private bool hasBeenOnScreen;
    private float offscreenSince = -1f;
    private float spawnedAt;

    private void Start()
    {
        spawnedAt = Time.time;
    }

    private void Update()
    {
        if (Time.time - spawnedAt > MaxLifetimeSeconds)
        {
            Destroy(gameObject);
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
            return;

        // CameraRatio overrides the projection matrix but leaves orthographicSize intact, so this
        // is still the correct half-height of the view.
        float halfHeight = cam.orthographicSize;
        float y = transform.position.y;
        bool offscreen = y > halfHeight + EdgeMargin || y < -halfHeight - EdgeMargin;

        if (!offscreen)
        {
            hasBeenOnScreen = true;
            offscreenSince = -1f;
            return;
        }

        if (!hasBeenOnScreen)
            return; // Still on its way in from the spawn edge.

        if (offscreenSince < 0f)
            offscreenSince = Time.time;
        else if (Time.time - offscreenSince >= GraceSeconds)
            Destroy(gameObject);
    }
}

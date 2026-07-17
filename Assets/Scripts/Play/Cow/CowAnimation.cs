using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class CowAnimation : MonoBehaviour
{
    [HideInInspector]
    public Animator headAnimator;
    public Animator bodyAnimator;
    public Animator ligterHandAnimator;
    public GameObject Flame;

    PowerUpSpawner powerUpSpawner;

    // Mad-cow face morph: while "mad", cross-fade the head between its normal face and the "crazy"
    // face, easing smoothly back and forth a few times over the mad duration (not a hard flicker).
    const float MadCycles = 3f; // Full normal->mad->normal cross-fades over the mad duration.
    SpriteRenderer headRenderer;
    SpriteRenderer madOverlay;  // Drawn just above the head; its alpha blends the crazy face in/out.
    Sprite normalFace;          // The cow's normal head sprite (captured at Start).
    Sprite madFace;             // The crazy face (captured from the animator's mad state at runtime).
    bool madActive;
    bool cowIsDead;
    float madElapsed;
    float madPeriod = 1.25f;    // Seconds per full back-and-forth (set from CowStats.madCowTime in Start).


    void Start()
    {
        powerUpSpawner = GameObject.Find("PowerUpSpawner").GetComponent<PowerUpSpawner>();
        if (powerUpSpawner == null)
            throw new MissingComponentException("CowAnimation needs a reference to the general PowerUpSpawner so the cow could react to a power up being taken.");

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gameManager == null)
            throw new MissingComponentException("CowAnimation needs a reference to the general GameController so the cow could react to the game being won.");

        gameManager.CowWon += CowWon;
        powerUpSpawner.PowerUpUsed += PowerUpUsed;
        GetComponent<CowShooting>().HasTakenDamage += HasTakenDamage;
        GetComponent<MadCowEffect>().MadCow += MadCow;
        GetComponent<CowHealth>().CowDead += CowDead;
        GetComponent<Movement>().GoingUp += GoingUp;

        headRenderer = headAnimator != null ? headAnimator.GetComponent<SpriteRenderer>() : null;
        if (headRenderer != null)
            normalFace = headRenderer.sprite; // The normal face, before any mad/hurt animation plays.

        CowStats stats = GetComponent<CowStats>();
        if (stats != null && stats.madCowTime > 0f)
            madPeriod = stats.madCowTime / MadCycles;
    }

    private void HasTakenDamage(GameObject CowBeingHit)
    {
        Animator enemyAnimator = CowBeingHit.transform.Find("CowRenderers/CowHead").GetComponent<Animator>();
        if (enemyAnimator == null)
            throw new MissingComponentException("Enemy cow needs the head animator");

        enemyAnimator.SetTrigger("GotHurt");
    }



    private void PowerUpUsed(GameObject PowerUp, GameObject EffectedCow)
    {
        if (EffectedCow.transform.name != gameObject.transform.name)
            return;

        IPowerUp PowerUpBehavior = PowerUp.GetComponent(typeof(IPowerUp)) as IPowerUp;

        if (PowerUpBehavior == null)
            throw new MissingComponentException("The power ups needs to have a script that inherits from IPowerUp");

        if (PowerUpBehavior.PowerUpEffect() == PowerUpEffect.Good)
            headAnimator.SetTrigger("GotPowerUp");
    }

    private void MadCow(bool MadCowStatus)
    {
        headAnimator.SetBool("IsMadCow", MadCowStatus);

        madActive = MadCowStatus && !cowIsDead;
        if (MadCowStatus)
        {
            madElapsed = 0f;
        }
        else
        {
            HideMadOverlay();
            if (headRenderer != null && normalFace != null && !cowIsDead)
                headRenderer.sprite = normalFace; // Rest on the normal face once the madness wears off.
        }
    }

    // Runs after the Animator each frame, so writing the head sprite / overlay here overrides the
    // static mad clip. The head stays on the normal face while an overlay cross-fades the crazy face
    // in and out, smoothly morphing between the two phases a few times across the mad duration.
    private void LateUpdate()
    {
        if (!madActive || headRenderer == null)
            return;

        // Lazily grab the crazy face the animator's mad state applies, so we need no asset reference.
        if (madFace == null)
        {
            if (headRenderer.sprite != null && headRenderer.sprite != normalFace)
                madFace = headRenderer.sprite;
            else
                return; // Animator hasn't switched to the mad face yet — let it show one frame.
        }

        headRenderer.sprite = normalFace; // Base head is the normal face; overlay blends the crazy one.

        SpriteRenderer overlay = EnsureMadOverlay();
        if (overlay == null)
            return;

        overlay.sprite = madFace;
        overlay.flipX = headRenderer.flipX;
        overlay.flipY = headRenderer.flipY;
        overlay.enabled = true;

        madElapsed += Time.deltaTime;
        // Smooth 0 -> 1 -> 0 each period, then shaped (smootherstep) so it dwells at full-normal and
        // full-mad and passes quickly through the mid-blend. MadCycles of these over the duration.
        float wave = 0.5f * (1f - Mathf.Cos((madElapsed / madPeriod) * 2f * Mathf.PI));
        float blend = wave * wave * wave * (wave * (wave * 6f - 15f) + 10f);

        Color c = overlay.color;
        c.a = Mathf.Clamp01(blend);
        overlay.color = c;
    }

    private SpriteRenderer EnsureMadOverlay()
    {
        if (madOverlay == null && headRenderer != null)
        {
            GameObject go = new GameObject("MadFaceOverlay");
            go.transform.SetParent(headRenderer.transform, false); // Inherits the head's position/rotation/scale.
            madOverlay = go.AddComponent<SpriteRenderer>();
            madOverlay.sortingLayerID = headRenderer.sortingLayerID;
            madOverlay.sortingOrder = headRenderer.sortingOrder + 1;
            Color c = madOverlay.color;
            c.a = 0f;
            madOverlay.color = c;
        }
        return madOverlay;
    }

    private void HideMadOverlay()
    {
        if (madOverlay == null)
            return;
        Color c = madOverlay.color;
        c.a = 0f;
        madOverlay.color = c;
        madOverlay.enabled = false;
    }

    private void CowDead(GameObject DeadCow)
    {
        // Stop the mad morph so the death animation's face isn't overwritten in LateUpdate.
        cowIsDead = true;
        madActive = false;
        HideMadOverlay();

        headAnimator.SetTrigger("Dead");
        ligterHandAnimator.SetTrigger("Dead");
        bodyAnimator.SetTrigger("Dead");

        UnityEngine.Object blood = Resources.Load("DeathBloodFromNeck");
        Transform headTransform = headAnimator.transform;
        Vector3 newHeadPosition = new Vector3(headTransform.position.x, headTransform.position.y - 0.5f, -5);
        Instantiate(blood, newHeadPosition, new Quaternion());

    }

    private void CowWon(GameObject WinningCow)
    {
        if (WinningCow.transform.name != gameObject.transform.name)
            return;

        headAnimator.SetTrigger("Won");
    }

    private void GoingUp(bool flameOn)
    {
        Flame.SetActive(flameOn);
    }
}

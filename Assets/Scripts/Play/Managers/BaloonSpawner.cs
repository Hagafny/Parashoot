using UnityEngine;
using System.Collections;
using System;

public class BaloonSpawner : MonoBehaviour
{

    public GameManager gameManager;
    public BaloonOptions[] Baloons;				// Array of powerup options
    public GameObject BaseBaloon;
    public float baloonDeliveryTime = 8f;		// Delay on delivery.
    public float RangeLeft;					// Smallest value of x in world coordinates the delivery can happen at.
    public float RangeRight;                // Largest value of x in world coordinates the delivery can happen at.

    void Start()
    {
        // Tuned cadence wins over the scene object's serialized value (binary scene — see
        // GameplayTuning). Set before the coroutine starts so the very first wait uses it.
        baloonDeliveryTime = GameplayTuning.BalloonInterval;

        // Start the first delivery.
        StartCoroutine(BaloonSpawn());
        RegisterToEvents();
    }


    public IEnumerator BaloonSpawn()
    {
        while (true) //Go on forever!! (This is temporary until I find a better thing to write here.
        {
            // Wait for the delivery delay.
            yield return new WaitForSeconds(baloonDeliveryTime);

            // Create a random x coordinate for the delivery in the drop range.
            float dropPosX = UnityEngine.Random.Range(RangeLeft, RangeRight);

            // Create a position with the random x coordinate.
            Vector3 dropPos = new Vector3(dropPosX, -19f, 1f);
            int baloonIndex = UnityEngine.Random.Range(0, Baloons.Length);
            // ... instantiate the base cloud at the drop position.
            Baloons[baloonIndex].Instance = Instantiate(BaseBaloon, dropPos, Quaternion.identity) as GameObject;
            Baloons[baloonIndex].Setup();

            ScaleRise(Baloons[baloonIndex].Instance);
        }


    }


    /// <summary>
    /// Scales the balloon's authored buoyancy to control how fast it climbs. Multiplies rather
    /// than replaces gravityScale: the prefab's value lives in a binary asset and can't be read
    /// from source, so scaling keeps whatever it intended (including the sign, which is what makes
    /// the balloon rise) and only changes the rate.
    ///
    /// Currently scaled DOWN — at the prefab's authored gravity a balloon accelerates past bullet
    /// speed before it clears the screen, which reads as physically wrong and pulls the eye off
    /// the actual threat.
    ///
    /// Popping is unaffected — ACowHasShotABalloon assigns gravityScale = 20 outright.
    /// </summary>
    private void ScaleRise(GameObject balloon)
    {
        Rigidbody2D balloonRB = balloon.GetComponent<Rigidbody2D>();
        if (balloonRB == null)
            throw new MissingComponentException("Ballon needs a Rigidbody");

        balloonRB.gravityScale *= GameplayTuning.BalloonRiseMultiplier;

        GiveOffscreenLifetime(balloon);
    }

    /// <summary>
    /// Replaces any fixed self-destruct timer with one that measures when the object actually
    /// leaves the view.
    ///
    /// The prefab's SelfDestructBySeconds was authored against the prefab's original (fast) rise.
    /// Slowing the balloon down meant that timer expired while the balloon was still mid-screen,
    /// so balloons vanished in front of the player. Disabling it before its Start runs prevents
    /// the Destroy call from ever being scheduled — Unity skips Start on a disabled component, and
    /// a Destroy(obj, t) already scheduled cannot be cancelled.
    /// </summary>
    private void GiveOffscreenLifetime(GameObject spawned)
    {
        // Root only. Children may legitimately use SelfDestructBySeconds for effect cleanup
        // (pop animations, explosions), and those should keep their own timing.
        SelfDestructBySeconds fixedTimer = spawned.GetComponent<SelfDestructBySeconds>();
        if (fixedTimer != null)
        {
            fixedTimer.enabled = false;
            Destroy(fixedTimer);
        }

        OffscreenLifetime lifetime = spawned.AddComponent<OffscreenLifetime>();
        lifetime.GraceSeconds = GameplayTuning.OffscreenGraceSeconds;
    }

    void RegisterToEvents()
    {
        gameManager.ACowHasShotABalloon += ACowHasShotABalloon;

    }

    private void ACowHasShotABalloon(GameObject TheBalloon, GameObject TheBullet, GameObject ShootingCow)
    {
        RemoveBallonColliders(TheBalloon);
        MakeBalloonSymbolDisappear(TheBalloon);
        MakeBalloonFall(TheBalloon);
        MakePopSound(TheBalloon);
        AnimateBalloonPop(TheBalloon);
        ImplementBalloonBehaviour(TheBalloon, TheBullet);
        MakeSpesificBalloonSound(TheBalloon);
    }

    private void RemoveBallonColliders(GameObject TheBalloon)
    {
        Collider2D[] colliders = TheBalloon.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false; 
        }
    }
    private void MakeBalloonSymbolDisappear(GameObject TheBalloon)
    {
        SpriteRenderer balloonSpriteRenderer = TheBalloon.transform.Find("Symbol").GetComponent<SpriteRenderer>();
        if (balloonSpriteRenderer == null)
        {
            throw new MissingComponentException("Balloon's symbol needs a sprite renderer");
        }

        balloonSpriteRenderer.enabled = false;
    }
    private void MakeBalloonFall(GameObject TheBalloon)
    {
        Rigidbody2D balloonRB = TheBalloon.GetComponent<Rigidbody2D>();
        if (balloonRB == null)
        {
            throw new MissingComponentException("Ballon needs a Rigidbody");
        }

        balloonRB.gravityScale = 20;
    }

    private void AnimateBalloonPop(GameObject TheBalloon)
    {
        Animator balloonAnimator = TheBalloon.GetComponent<Animator>();
        if (balloonAnimator == null)
        {
            throw new MissingComponentException("Ballon needs an animator");
        }

        balloonAnimator.SetTrigger("BalloonPopped");
    }

    private void ImplementBalloonBehaviour(GameObject TheBalloon, GameObject TheBullet)
    {
        IBalloon balloonBehaviour = TheBalloon.GetComponent(typeof(IBalloon)) as IBalloon;
        if (balloonBehaviour == null)
        {
            throw new MissingComponentException("Balloon needs a script that implements IBalloon");
        }

        balloonBehaviour.Pop(TheBullet);
    }

    private void MakePopSound(GameObject TheBalloon)
    {
        TheBalloon.GetComponent<AudioSource>().Play();
    }

    private void MakeSpesificBalloonSound(GameObject TheBalloon)
    {
        IBalloon balloonBehaviour = TheBalloon.GetComponent(typeof(IBalloon)) as IBalloon;
        if (balloonBehaviour == null)
        {
            throw new MissingComponentException("Balloon needs a script that implements IBalloon");
        }

        balloonBehaviour.StartBalloonSound();
    }
}

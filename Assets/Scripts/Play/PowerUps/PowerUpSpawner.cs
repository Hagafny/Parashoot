using UnityEngine;
using System.Collections;
using System;
public class PowerUpSpawner : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject PowerUpContainer;
    public PowerUpManager[] PowerUps;				// Array of powerup options
    public float powerUpDeliveryTime = 15f;		// Delay on delivery.
    public float dropRangeLeft;					// Smallest value of x in world coordinates the delivery can happen at.
    public float dropRangeRight;				// Largest value of x in world coordinates the delivery can happen at.
    public Action<GameObject, GameObject> PowerUpUsed;
    void Start()
    {
        // Tuned cadence wins over the scene object's serialized value (binary scene — see
        // GameplayTuning). Set before the coroutine starts so the very first wait uses it.
        powerUpDeliveryTime = GameplayTuning.PowerUpInterval;

        // Start the first delivery.
        StartCoroutine(PowerUpPickup());

        RegisterToEvents();
    }

    /// <summary>
    /// Scales the crate's authored gravity to control how fast it descends. Multiplies rather than
    /// replaces gravityScale, for the same reason as the balloons: the prefab value is in a binary
    /// asset and can't be read from source, so scaling preserves its intent and changes only the rate.
    ///
    /// Currently scaled DOWN — it's a parachuted crate, so it should drift rather than plummet,
    /// and a slow descent is what gives both players time to decide whether to contest it.
    ///
    /// BoxExplosion is unaffected — it assigns -ParachuteNegativeGravity outright once the box
    /// is shot away, so the freed parachute still flies up exactly as before.
    /// </summary>
    private void ScaleFall(GameObject crate)
    {
        Rigidbody2D crateRB = crate.GetComponent<Rigidbody2D>();
        if (crateRB == null)
            return; // Gravity stays as the prefab authored it.

        crateRB.gravityScale *= GameplayTuning.CrateFallMultiplier;

        GiveOffscreenLifetime(crate);
    }

    /// <summary>
    /// Same treatment as the balloons: swap any fixed self-destruct timer for one that measures
    /// when the crate actually leaves the view, so retuning CrateFallMultiplier can never again
    /// make crates vanish mid-screen.
    ///
    /// Disabling the component before its Start runs is what prevents the Destroy from being
    /// scheduled — an already-scheduled Destroy(obj, t) cannot be cancelled.
    /// </summary>
    private void GiveOffscreenLifetime(GameObject spawned)
    {
        // Root only — children use SelfDestructBySeconds for effect cleanup (the box explosion),
        // and that timing should be left alone.
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
        gameManager.ACowHasShotAPowerUp += ACowHasShotAPowerUp;
    }
    public IEnumerator PowerUpPickup()
    {
        while (true) //Go on forever!! (This is temporary until I find a better thing to write here.
        {
            // Wait for the delivery delay.
            yield return new WaitForSeconds(powerUpDeliveryTime);

            // Create a random x coordinate for the delivery in the drop range.
            float dropPosX = UnityEngine.Random.Range(dropRangeLeft, dropRangeRight);

            // Create a position with the random x coordinate.
            Vector3 dropPos = new Vector3(dropPosX, 19f, 1f);

            // ... instantiate a random pickup at the drop position.
            int pickupIndex = UnityEngine.Random.Range(0, PowerUps.Length);
            PowerUps[pickupIndex].Instance = Instantiate(PowerUpContainer, dropPos, Quaternion.identity) as GameObject;
            PowerUps[pickupIndex].Setup();

            ScaleFall(PowerUps[pickupIndex].Instance);

            PowerUpCollision powerUpCollisionScript = PowerUps[pickupIndex].Instance.GetComponentInChildren<PowerUpCollision>();

            if (powerUpCollisionScript == null)
                throw new MissingComponentException("Power up instasnce needs a reference to PowerUpCollision in his Symbol child");

            powerUpCollisionScript.CowReceivedPowerUp += CowReceivedPowerUp;
        }


    }


    private void ACowHasShotAPowerUp(GameObject PowerUp, GameObject ShootingCow, GameObject EnemyCow)
    {
        Animator boxAnimator = PowerUp.transform.Find("Container/Box").GetComponent<Animator>();
        boxAnimator.SetTrigger("ContainerDestroyed");

        BoxCollider2D boxCollider = PowerUp.transform.GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;

        Transform PowerUpItem = PowerUp.transform.Find("PowerUpItem");
        PowerUpItem.parent = null;

        IPowerUp PowerUpBehavior = PowerUpItem.GetComponent(typeof(IPowerUp)) as IPowerUp;

        if (PowerUpBehavior == null)
            throw new MissingComponentException("This power up needs a script component that implements IPowerUp");

        Transform target = PowerUpBehavior.PowerUpEffect() == PowerUpEffect.Good ? ShootingCow.transform : EnemyCow.transform;
        MoveTowards moveTowardsScript = PowerUpItem.GetComponent<MoveTowards>();
        moveTowardsScript.Target = target.transform;

        PlaySmashSound(PowerUp);
    }

    private void CowReceivedPowerUp(GameObject PowerUp, GameObject EffectedCow)
    {
        IPowerUp PowerUpBehavior = PowerUp.GetComponent(typeof(IPowerUp)) as IPowerUp;

        if (PowerUp == null)
            throw new MissingComponentException("This power up needs a script component that implements IPowerUp");

        PowerUpBehavior.Use(EffectedCow);

        if (PowerUpUsed != null)
            PowerUpUsed(PowerUp, EffectedCow);
    }

    public void PlaySmashSound(GameObject PowerUp)
    {
        PowerUp.GetComponent<AudioSource>().Play();
    }
}


using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class CowShooting : MonoBehaviour
{

    //  public int m_PlayerNumber = 1; // This will be set from the CowManager

    public Rigidbody2D m_Bullet; // Reference to the bullet's RigidBody2D
    public Transform m_FireTransform; // Reference to the FirePoint of the gun.
    public AudioClip shotSound;


    [HideInInspector]
    public Action<GameObject> HasTakenDamage;
    [HideInInspector]
    public Action<GameObject, GameObject> ShooterHitPowerUp;
    [HideInInspector]
    public Action<GameObject, GameObject, GameObject> ShooterHitBalloon;
    [HideInInspector]
    public Action<GameObject> ShooterHitShield;

    CowStats stats;
    CowHealth cowHealth;
    IShooting shootingController;
    private AudioSource m_AudioSource;
    private float cooldownTimer = 0;

    void Awake()
    {
        stats = GetComponent<CowStats>();
        m_AudioSource = GetComponent<AudioSource>();
        cowHealth = GetComponent<CowHealth>();
    }
    void Start()
    {
        shootingController = GetComponent(typeof(IShooting)) as IShooting;
    }
    private void Update()
    {
        Shoot();
    }

    private void Shoot()
    {
        cooldownTimer -= Time.deltaTime;

        //Check if the player has clicked on the shot botton
        bool hasCowShot = shootingController.HasShot();

        //If it's ok for us to fire and we clicked either of the shots, shoot.
        if (cooldownTimer <= 0 && hasCowShot)
        {
            //Extend the cooldownTimer.
            cooldownTimer = stats.fireDelay;
            Fire();

        }
    }

    /// <summary>
    /// Calculates, insantiates and launches the bullet
    /// </summary>
    /// <param name="shotType">Up, Middle or Down</param>
    private void Fire()
    {
        //Instantiate the bullet at the fireTransform position and the Quaterntion we have just calculated
        Rigidbody2D bulletInstance = Instantiate(m_Bullet, m_FireTransform.position, transform.rotation) as Rigidbody2D;

        BulletMovement bulletMovement = bulletInstance.GetComponent<BulletMovement>();


        bulletMovement.BulletHitPlayer += BulletHitPlayer;
        bulletMovement.BulletHitPowerUp += BulletHitPowerUp;
        bulletMovement.BulletHitBalloon += BulletHitBalloon;
        bulletMovement.BulletHitShield += BulletHitShield;
        bulletMovement.BulletHitShieldExit += BulletHitShieldExit;


        //Play the sound effect
        m_AudioSource.PlayOneShot(shotSound);
    }

    private void BulletHitPlayer(GameObject PlayerBeingHit)
    {
        if (HasTakenDamage != null && !cowHealth.m_Dead)
        {
            HasTakenDamage(PlayerBeingHit);
        }
    }

    private void BulletHitPowerUp(GameObject PowerUpBeingHit)
    {
        if (ShooterHitPowerUp != null)
            ShooterHitPowerUp(PowerUpBeingHit, gameObject);
    }
    private void BulletHitBalloon(GameObject BalloonBeingHit, GameObject TheBullet)
    {
        if (ShooterHitBalloon != null)
            ShooterHitBalloon(BalloonBeingHit, TheBullet, gameObject);
    }

    private void BulletHitShield(GameObject bullet, string CowWhoHasShieldName)
    {
        BulletMovement bulletMovementScript = bullet.GetComponent<BulletMovement>();

        if (ShooterHitShield != null && (CowWhoHasShieldName != transform.name || bulletMovementScript.ricochet))
        {
            ShooterHitShield(gameObject);
        }
    }

    private void BulletHitShieldExit(GameObject bullet, string CowWhoHasShieldName)
    {
        BulletMovement bulletMovementScript = bullet.GetComponent<BulletMovement>();
        if (CowWhoHasShieldName != transform.name || bulletMovementScript.ricochet)
        {
            bulletMovementScript.ricochet = !bulletMovementScript.ricochet;

            Vector2 v = bullet.GetComponent<Rigidbody2D>().velocity;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(180 + angle, Vector3.forward);
        }
    }
}
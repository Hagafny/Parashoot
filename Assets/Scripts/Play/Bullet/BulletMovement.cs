using UnityEngine;
using System.Collections;
using System;
public class BulletMovement : MonoBehaviour
{
    public float bulletSpeed = 24f; // Speed of the bullet
    public Action<GameObject> BulletHitPlayer;
    public Action<GameObject> BulletHitPowerUp;
    public Action<GameObject, GameObject> BulletHitBalloon;
    public Action<GameObject, string> BulletHitShield;
    public Action<GameObject, string> BulletHitShieldExit;
    public GameObject BulletExplosion;
    public GameObject BulletBlood;

    [HideInInspector]
    public bool ricochet = false;
    Rigidbody2D rb;
    float bulletRegularMass;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //Add speed to the bullet.
        rb.velocity = transform.right * bulletSpeed * -1; //I seem to need the -1 to avoid the cows hitting themselves.
        bulletRegularMass = rb.mass;

    //    transform.rotation = Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.y);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        bool destroyBullet = true;

        string collisionTag = other.gameObject.tag;
        //Hitting the enemy cow.

        if (collisionTag == "Bullet")
        {
            float thisBulletMass = rb.mass;
            float enemyBulletMass = other.gameObject.GetComponent<Rigidbody2D>().mass;

            if (thisBulletMass > enemyBulletMass)
                destroyBullet = false;
            else
                Instantiate(BulletExplosion, transform.position, transform.rotation * Quaternion.Euler(0, 0, -90));
        }

        if (collisionTag == "Player")
        {
            CowHealth enemyLife = other.gameObject.GetComponent<CowHealth>();
            bool shield = enemyLife.m_Shield;
            bool invincible = enemyLife.m_Invincibe;
            if (shield)
            {
                destroyBullet = false;
                BulletHitShield(transform.gameObject, other.transform.name);
            }
            else if (!invincible)
            {
                if (BulletHitPlayer != null)
                    BulletHitPlayer(other.gameObject);

                Instantiate(BulletBlood, transform.position, other.transform.rotation);
            }
        }
        //Hitting a powerup
        if (collisionTag == "PowerUp" && BulletHitPowerUp != null)
        {
            //If the bullet has been enlarged, don't destroy the bullet - go through the Power Up
            if (rb.mass > bulletRegularMass)
                destroyBullet = false;

            BulletHitPowerUp(other.gameObject);
        }



        if (destroyBullet)
        {
            //Destroy the bullet. I was having trouble because one one hand, if we immediately destroy the bullet, we won't get the ricochet effect we wanted (making enemy cow move as a result of the hit)
            //On the other hand, if I didn't destroy the bullet, it will just keep going. Eventually I found this solution, destroy the object but only after 0.1 second and also turn of his sprite.
            //This seemingly vanishes the bullet but will actually destroy it after 0.1 seconds. This way we also get the ricochet effect and everyone is happy.
            Destroy(gameObject, 0.1f);
            gameObject.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            CowHealth enemyLife = other.gameObject.GetComponent<CowHealth>();
            bool shield = enemyLife.m_Shield;
            if (shield && BulletHitShieldExit != null)
            {
                BulletHitShieldExit(transform.gameObject, other.transform.name);
            }
            
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //Hitting a balloon
        if (other.gameObject.tag == "Balloon" && BulletHitBalloon != null)
        {
            BulletHitBalloon(other.gameObject, gameObject);
        }
    }

    void ColliderOn()
    {
        GetComponent<Rigidbody2D>().mass *= 10;
        GetComponent<Collider2D>().enabled = true;
    }



}

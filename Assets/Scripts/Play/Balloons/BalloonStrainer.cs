using UnityEngine;
using System.Collections;
using System;


public class BalloonStrainer : MonoBehaviour, IBalloon
{

    public GameObject shot;
    public float offset = 20;
    public float changeInYAxix = 1;
    UnityEngine.Object bulletPrefab;
    BulletMovement originalBulletScript;
    public AudioClip startingSound;


    void Start()
    {
        bulletPrefab = Resources.Load("Bullet");
    }

    public void Pop(GameObject bullet)
    {
        // get the data from the original bullet
        originalBulletScript = bullet.GetComponent<BulletMovement>();

        // set the location of the first bullet
        Vector3 bulletPosition1 = bullet.transform.position;
        bulletPosition1.y += changeInYAxix;

        //set location of the second bullet
        Vector3 bulletPosition2 = bullet.transform.position;
        bulletPosition2.y -= changeInYAxix;


        //set the rotation of the bullet

        Quaternion bulletQuaternion = bullet.transform.rotation;
        int angleFactor = bullet.transform.rotation.eulerAngles.y >= 180 ? 1 : -1;
        int velocityFactor = bullet.GetComponent<Rigidbody2D>().velocity.x > 0 ? 1 : -1;
        SpawnBullet(bulletPosition1, bulletQuaternion * Quaternion.Euler(0, 0, -offset * angleFactor * velocityFactor));
        SpawnBullet(bulletPosition2, bulletQuaternion * Quaternion.Euler(0, 0, offset * angleFactor * velocityFactor));

    }

    private void SpawnBullet(Vector3 newBulletPosition, Quaternion newBulletRotation)
    {
        GameObject shot = Instantiate(bulletPrefab, newBulletPosition, newBulletRotation) as GameObject;
        AttachEvents(shot);
    }

    private void AttachEvents(GameObject newBullet)
    {
        BulletMovement newBulletScript = newBullet.GetComponent<BulletMovement>();
        newBulletScript.BulletHitPlayer = originalBulletScript.BulletHitPlayer;
        newBulletScript.BulletHitBalloon = originalBulletScript.BulletHitBalloon;
        newBulletScript.BulletHitPowerUp = originalBulletScript.BulletHitPowerUp;
        newBulletScript.BulletHitShield = originalBulletScript.BulletHitShield;
        newBulletScript.BulletHitShieldExit = originalBulletScript.BulletHitShieldExit;
    }

    public void StartBalloonSound()
    {
        GetComponent<AudioSource>().PlayOneShot(startingSound);
    }
}


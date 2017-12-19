using UnityEngine;
using System.Collections;

/// <summary>
/// The method SelfDestory gets called when the box explode animation is done. 
/// </summary>
public class BoxExplosion : MonoBehaviour {

    public float ParachuteNegativeGravity = 25f;
    public void SelfDestroy()
    {
        //The Box component is destroyed so we reach for the parent RigidBody and reduce the gravity scale. This means that the parachute now flys upwards
        gameObject.transform.GetComponentInParent<Rigidbody2D>().gravityScale = -ParachuteNegativeGravity;
        Destroy(gameObject);
    }
}

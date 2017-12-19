using UnityEngine;
using System.Collections;
using System;
public class PowerUpCollision : MonoBehaviour {

    public Action<GameObject, GameObject> CowReceivedPowerUp; 
    void OnTriggerEnter2D(Collider2D other)
    {
        //Hitting the cow.
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);

            bool cowDead = other.gameObject.GetComponent<CowHealth>().m_Dead;
            if (CowReceivedPowerUp != null && !cowDead)
                CowReceivedPowerUp(gameObject, other.gameObject);
        }
    }
}

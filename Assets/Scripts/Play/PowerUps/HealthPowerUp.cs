using UnityEngine;
using System.Collections;

public class HealthPowerUp : MonoBehaviour, IPowerUp {
   // private AudioSource m_AudioSource; 
	void Start () {
      //  m_AudioSource = GetComponent<AudioSource>();
	}
	
    /// <summary>
    /// The Health Updgrade method of the IPowerUp. This occurs when a bullet collides with the health power up.
    /// </summary>
    /// <param name="cow"></param>
    public PowerUpEffect PowerUpEffect()
    {
        return global::PowerUpEffect.Good;
    }
    public void Use(GameObject EffectedCow)
    {


        //Check if the cow has a CowHealth component script attached. If it does, add a life.
        CowHealth cowHealth = EffectedCow.GetComponent<CowHealth>();

        if (cowHealth == null)
            return;

        cowHealth.AddHealth(1); //Add a life
    }

    public PowerUpType GetPowerUpType()
    {
        return PowerUpType.Health;
    }


}

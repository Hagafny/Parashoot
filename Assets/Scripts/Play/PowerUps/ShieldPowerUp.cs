using UnityEngine;
using System.Collections;
using System;

public class ShieldPowerUp : MonoBehaviour, IPowerUp {

    public PowerUpEffect PowerUpEffect()
    {
        return global::PowerUpEffect.Good;
    }
    public void Use(GameObject EffectedCow)
    {

        EffectedCow.GetComponent<ShieldEffect>().activateShield();
    }

    public PowerUpType GetPowerUpType()
    {
        return PowerUpType.Shield;
    }
}

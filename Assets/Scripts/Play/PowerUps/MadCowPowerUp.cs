using UnityEngine;
using System.Collections;
using System;

public class MadCowPowerUp : MonoBehaviour, IPowerUp
{
    public PowerUpEffect PowerUpEffect()
    {
        return global::PowerUpEffect.Bad;
    }

    /// <summary>
    /// This method attaches the mad cow effect to the effected cow and calls Start Mad Cow.
    /// </summary>
    /// <param name="EffectedCow"></param>
    public void Use(GameObject EffectedCow)
    {
        EffectedCow.GetComponent<MadCowEffect>().activateMadCow();
    }

    public PowerUpType GetPowerUpType()
    {
        return PowerUpType.MadCow;
    }
}

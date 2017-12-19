using UnityEngine;
using System.Collections;

public interface IPowerUp
{
    PowerUpEffect PowerUpEffect();
    void Use(GameObject EffectedCow);
    PowerUpType GetPowerUpType();
}

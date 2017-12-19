using UnityEngine;
using System.Collections;
using System;


public enum PowerUpType
{
    Health,
    Shield,
    MadCow
}

public enum PowerUpDirection
{
    Player,
    Enemy
}

[Serializable]
public class PowerUpManager
{
    public PowerUpType Type;
    public Sprite Symbol;
    public Color ParachuteColor;
    public Vector2 Scale;

    [HideInInspector]
    public GameObject Instance;

    Transform powerUp;

    public void Setup()
    {
        if (Instance == null)
            return;

        powerUp = Instance.transform.Find("PowerUpItem");
        InjectBehavior();
        ImplementScale();
        HandleSprites();

    }

    private void InjectBehavior()
    {
        switch (Type)
        {
            case PowerUpType.Health:
                powerUp.gameObject.AddComponent<HealthPowerUp>();
                break;
            case PowerUpType.Shield:
                powerUp.gameObject.AddComponent<ShieldPowerUp>();
                break;
            case PowerUpType.MadCow:
                powerUp.gameObject.AddComponent<MadCowPowerUp>();
                break;
            default:
                break;
        }
    }

    private void ImplementScale()
    {
        powerUp.transform.localScale = Scale;
    }
    private void HandleSprites()
    {
        SpriteRenderer[] sprites = Instance.GetComponentsInChildren<SpriteRenderer>();
        int spritesLength = sprites.Length;
        for (int i = 0; i < spritesLength; i++)
        {
            string name = sprites[i].transform.name;
            if (name == "Parachute")
                sprites[i].color = ParachuteColor;

            if (name == "PowerUpItem")
                sprites[i].sprite = Symbol;
        }
    }
}

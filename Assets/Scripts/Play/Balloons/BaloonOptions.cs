using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class BaloonOptions {

    public Sprite Symbol;
    public BaloonEffect Effect;
    public Vector3 symbolPosition = Vector3.zero;
    [HideInInspector]
    public GameObject Instance;
    public AudioClip symbolSound;


    public void Setup()
    {
        SetSymbolSprite();
        SetSymbolPosition();
        SetBehaviour();
    }

    private void SetSymbolSprite()
    {
        SpriteRenderer symbolRenderer = Instance.transform.Find("Symbol").GetComponent<SpriteRenderer>();
        if (symbolRenderer == null)
        {
            throw new MissingComponentException("the symbol component of the balloon is missing as SpriteRenderer");
        }
        symbolRenderer.sprite = Symbol;
    }

    private void SetBehaviour()
    {
        switch (Effect)
        {   
            case BaloonEffect.MagnifyingGlass:
                BalloonMagnfyingGlass MagnifyingScript = Instance.AddComponent<BalloonMagnfyingGlass>();
                MagnifyingScript.startingSound = symbolSound; 
                break;
            case BaloonEffect.Strainer:
                BalloonStrainer StrainerScript = Instance.AddComponent<BalloonStrainer>();
                StrainerScript.startingSound = symbolSound;
                break;
            default:
                break;
        }
    }
    private void SetSymbolPosition()
    {
         Instance.transform.Find("Symbol").transform.localPosition = symbolPosition;
    }
}

public enum BaloonEffect
{
    MagnifyingGlass,
    Strainer
}


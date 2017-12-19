using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class CloudOptions
{

    public Vector2 Scale = new Vector2(1f,1f);
    public Sprite Sprite;

    public GameObject SetupCloud(GameObject cloud)
    {
        cloud.transform.localScale = Scale;
        cloud.GetComponent<SpriteRenderer>().sprite = Sprite;

        return cloud;
    }
}

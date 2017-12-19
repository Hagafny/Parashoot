using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ColorPlaces 
{
    public Color color;
    public Sprite ribbonImage;

    public ColorPlaces(Color Color, Sprite RibbonImage)
    {
        this.color = Color;
        this.ribbonImage = RibbonImage;
    }
}

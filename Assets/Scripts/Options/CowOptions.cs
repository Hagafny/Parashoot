using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
    [Serializable]
public class CowOptions {
	public string name;
	public Color color;
    public CowType type;

    public CowOptions(string Name, Color Color, CowType Type)
    {
        this.name = Name;
        this.color = Color;
        this.type = Type; 
    }

}

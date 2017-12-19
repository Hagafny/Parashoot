using UnityEngine;
using System.Collections;
using System;

public class ColorPick
{
    public Players player;
    public CowColor color;
    public ColorPick(string colorPick)
    {
        string[] colorArguments = colorPick.Split(',');
        if (colorArguments.Length != 2)
            throw new Exception("Something is wrong with the argument string. It should be <PlayerNumber>,<ColorName>");

        player = (Players)int.Parse(colorArguments[0]);
        color = (CowColor)System.Enum.Parse(typeof(CowColor), colorArguments[1]);
    }
}

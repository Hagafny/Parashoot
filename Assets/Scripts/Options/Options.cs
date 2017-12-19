using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class Options : MonoBehaviour
{
    public CowOptionsParts[] CowOptionParts;
    public InputField[] NameInputs;
    public GameObject[] ColorBars;
    public RibbonImages RibbonImages;

    GameOptions gameOptions;

    private Color RedColor = new Color32(191, 44, 51, 255);
    private Color BlueColor = new Color32(32, 116, 152, 255);
    private Color GreenColor = new Color32(73, 206, 154, 255);
    private Color PinkColor = new Color32(233, 98, 143, 255);
    private Color PurpleColor = new Color32(184, 104, 248, 255);
    private Color YellowColor = new Color32(222, 220, 55, 255);

    private Dictionary<CowColor, ColorPlaces> Colors;

    private string botName = "Cowputer";

    public void Start()
    {
        if (CowOptionParts.Length != 2)
            throw new Exception("CowOptionParts should be 2");

        gameOptions = GameObject.FindGameObjectWithTag("GameOptions").GetComponent<GameOptions>();

        InitiateRibbonDictionary();

        InitializeValues();

    }

    private void InitiateRibbonDictionary()
    {
        Colors = new Dictionary<CowColor, ColorPlaces>
        {
            { CowColor.Red, new ColorPlaces(RedColor, RibbonImages.Red) },
            { CowColor.Blue, new ColorPlaces(BlueColor, RibbonImages.Blue) },
            { CowColor.Green, new ColorPlaces(GreenColor, RibbonImages.Green) },
            { CowColor.Pink, new ColorPlaces(PinkColor, RibbonImages.Pink) },
            { CowColor.Purple, new ColorPlaces(PurpleColor, RibbonImages.Purple) },
            { CowColor.Yellow, new ColorPlaces(YellowColor, RibbonImages.Yellow) }
        };
    }

    private void InitializeValues()
    {
        int cowOptionsLength = gameOptions.cowOptions.Length;
        for (int i = 0; i < cowOptionsLength; i++)
        {
            Players player = (Players)i + 1;
            CowOptions cowOption = gameOptions.cowOptions[i];

            InitializeName(player, cowOption.name);

            foreach (ColorPlaces colors in Colors.Values)
                if (colors.color == cowOption.color)
                    ChangeColors(player, colors);
        }
    }

    public void ToggleColorDropDown(GameObject dropDown)
    {
        dropDown.SetActive(!dropDown.activeSelf);
    }

    public void OnPointerEnterColor(string colorPick)
    {
        ColorPick cp = new ColorPick(colorPick);
        ChangeColors(cp.player, Colors[cp.color]);
    }

    public void ColorDropDownClick(string colorPick)
    {
        ColorPick cp = new ColorPick(colorPick);

        int playerNumber = cp.player.GetHashCode() - 1;
        SetColorGameOption(cp.player, Colors[cp.color].color);
        ToggleColorDropDown(ColorBars[playerNumber]);
    }

    public void OnPointerExitBar(int playerNumber)
    {
        Players player = (Players)playerNumber;
        CowOptions cowOption = gameOptions.cowOptions[playerNumber - 1];
        foreach (var colorsDic in Colors)
            if (colorsDic.Value.color == cowOption.color)
                ChangeColors(player, Colors[colorsDic.Key]);

    }

    #region Colors
    public void ChangeColors(Players Player, ColorPlaces Color)
    {
        CowOptionsParts cowParts = CowOptionParts[Player.GetHashCode() - 1]; //It's zero based so we decrease by one.


        ChangeColorIndicator(cowParts.ColorIndicator, Color.color);


        ChangeRibbon(cowParts.Ribbon, Color.ribbonImage);
    }

    private void ChangeColorIndicator(GameObject colorIndicator, Color color)
    {
        colorIndicator.GetComponent<Image>().color = color;
    }
    private void ChangeRibbon(GameObject ribbonLocation, Sprite Ribbon)
    {
        ribbonLocation.GetComponent<Image>().sprite = Ribbon;
    }

    #endregion

    #region Names
    public void InitializeName(Players Player, string Name)
    {
        if (Name == "Player " + Player.GetHashCode() || Name == botName)
            Name = string.Empty;

        InputField nameInput = NameInputs[Player.GetHashCode() - 1]; //It's zero based so we decrease by one.
        nameInput.text = Name;
    }

    public void Player1EndInput(string Name)
    {
        SetNameGameOption(Players.Player1, Name);
    }

    public void Player2EndInput(string Name)
    {
        SetNameGameOption(Players.Player2, Name);
    }

    #endregion



    #region SetOptions

    private void SetColorGameOption(Players Player, Color color)
    {
        int playerNumber = Player.GetHashCode() - 1;
        gameOptions.cowOptions[playerNumber].color = color;
    }
    private void SetNameGameOption(Players Player, string Name)
    {
        int playerNumber = Player.GetHashCode() - 1;
        gameOptions.cowOptions[playerNumber].name = Name;
    }

    //Shit code.
    public void FinalizeOptions(int CowType)
    {
        for (int i = 0; i < gameOptions.cowOptions.Length; i++)
        {
            if (gameOptions.cowOptions[i].name == string.Empty)
                gameOptions.cowOptions[i].name = GetDefaultName(i + 1);

            if (i == 1)
            {
                CowType type = (CowType)CowType;
                gameOptions.cowOptions[1].type = type; 
                if (type == global::CowType.Human)
                {
                    if (gameOptions.cowOptions[i].name == botName)
                        gameOptions.cowOptions[i].name = GetDefaultName(i + 1);
                }
                else
                    gameOptions.cowOptions[1].name = botName;
                
            }
        }
    }

    private string GetDefaultName(int playerNumber)
    {
        return "Player " + playerNumber;
    }
    #endregion
}





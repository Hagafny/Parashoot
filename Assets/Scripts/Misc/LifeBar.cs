using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
public class LifeBar : MonoBehaviour
{
    public Sprite EmptyHeart;
    public Sprite FullHeart;
    public Image[] Hearts;

    private int maxLives = 5;

    void Awake()
    {
        if (Hearts.Length != 10)
            throw new Exception("We need to have 10 hearts");
    }

    public void InitializeBar(int startingLives)
    {
        int heartsLength = Hearts.Length;
        for (int i = 0; i < heartsLength; i++)
        {
            int fullHearts = startingLives;

            int heartNumber = i + 1;
            heartNumber = heartNumber <= maxLives ? heartNumber : heartNumber - maxLives;

            if (heartNumber <= fullHearts)
                ChangeHeartState(Hearts[i], FullHeart);
            else
                HideHeart(Hearts[i]);
        }
    }
    public void LifeUp(int playerNumber, int currentLives)
    {
        int heartNumber = playerNumber == 1 ? currentLives : currentLives + maxLives;
        heartNumber = heartNumber - 1;
        Image heart = Hearts[heartNumber];
        ShowHeart(heart);
        ChangeHeartState(heart, FullHeart);
    }
    public void LifeDown(int playerNumber, int currentLives)
    {
        int heartNumber = playerNumber == 1 ? currentLives : currentLives + maxLives;
        Image heart = Hearts[heartNumber];
        ChangeHeartState(heart, EmptyHeart);
    }

    private void HideHeart(Image heart)
    {
        ChangeHeartAlphaColor(heart, 0f);
    }
    private void ShowHeart(Image heart)
    {
        ChangeHeartAlphaColor(heart, 255f);
    }
    private void ChangeHeartAlphaColor(Image heart, float alpha)
    {
        Color color = heart.color;
        color.a = alpha;
        heart.color = color;
    }
    private void ChangeHeartState(Image heart, Sprite state)
    {
        heart.sprite = state;
    }
}
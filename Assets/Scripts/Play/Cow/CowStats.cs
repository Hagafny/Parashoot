using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class CowStats : MonoBehaviour  {

    [HideInInspector]
    public int playerNumber; //This can only be start from GameMangaer after spawning the cows

    public int maximumeLives = 5; // The maximum lives the cow can get to (After gaining help powerups)
    public int startingLives = 3; // The starting lives 

    public float movementSpeed = 12f; //Movement Speed 
    public float rotationSpeed = 10f; //Rotation speed  
    public float yMaxBounadry = 9.2f; // The cow can only go this high
    public float yMinBounadry = -12f; // The cow can only go this low
    public bool invertedMovement = false; // inverted movement/ai rotation - for the madCow

    [Range(0, 180)]
    public float rotationAngleLimit = 45f; //The max angles the cow can rotate to on either side

    [Range(0.0f, 2f)]
    public float fireDelay = 0.75f; // Delay of each bullet shot

    public float shieldTime = 5f; // Seconds for shield to be active

    public float madCowTime = 5f; // Seconds for mad cow to be active


}

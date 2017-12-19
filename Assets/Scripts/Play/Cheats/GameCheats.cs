using UnityEngine;
using System.Collections;

public class GameCheats : MonoBehaviour
{

    private string[] cheatCode;
    private int index;
    public BaloonSpawner bs;

    void Start()
    {
        // Code is "moo", user needs to input this in the right order
        cheatCode = new string[] { "m", "o", "o" };
        index = 0;
    }

    void Update()
    {
        // Check if any key is pressed
        if (Input.anyKeyDown)
        {
            // Check if the next key in the code is pressed
            if (Input.GetKeyDown(cheatCode[index]))
            {
                // Add 1 to index to check the next key in the code
                index++;
            }
            // Wrong key entered, we reset code typing
            else
            {
                index = 0;
            }
        }

        // If index reaches the length of the cheatCode string, 
        // the entire code was correctly entered
        if (index == cheatCode.Length)
        {
            // Cheat code successfully inputted!
            BalloonOverload();
            index = 0;
        }
    }

    void BalloonOverload()
    {
        StartCoroutine(BalloonOverloadCoroutine());
    }
    IEnumerator BalloonOverloadCoroutine()
    {
        float temp = bs.baloonDeliveryTime;
        bs.baloonDeliveryTime = 0.04f;   
        yield return new WaitForSeconds(3.5f);
        bs.baloonDeliveryTime = temp;  
    }
}
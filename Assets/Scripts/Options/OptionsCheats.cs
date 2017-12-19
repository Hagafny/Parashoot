using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class OptionsCheats : MonoBehaviour
{

    private string[] cheatCode;
    private int index;
    Options optionsScript;

    void Start()
    {
        // Code is "insane", user needs to input this in the right order
        cheatCode = new string[] { "i", "n", "s", "a", "n", "e" };
        index = 0;
        optionsScript = GetComponent<Options>();
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
            InsaneMode();
            index = 0;
        }
    }

    void InsaneMode()
    {
        optionsScript.FinalizeOptions(4);
        SceneManager.LoadScene("Play");
    }
}
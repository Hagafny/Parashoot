using UnityEngine;
using System.Collections;

public class ExitGame : MonoBehaviour {

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Exit();
    }

    public void Exit()
    {
        Application.Quit();
    }
}

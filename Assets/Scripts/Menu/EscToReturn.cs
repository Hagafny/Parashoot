using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class EscToReturn : MonoBehaviour
{
    public int sceneNumber;
    void Update()
    {
        // ESC (keyboard) or the East button (Circle on PlayStation, B on Xbox) goes back.
        Gamepad pad = Gamepad.current;
        bool cancelPressed = pad != null && pad.buttonEast.wasPressedThisFrame;

        if (Input.GetKeyDown(KeyCode.Escape) || cancelPressed)
            SceneManager.LoadScene(sceneNumber);
    }
}

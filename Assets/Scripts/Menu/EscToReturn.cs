using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class EscToReturn : MonoBehaviour
{
    public int sceneNumber;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(sceneNumber);
    }
}

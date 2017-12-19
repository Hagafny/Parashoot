using UnityEngine;
using UnityEngine.UI;// we need this namespace in order to access UI elements within our script
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class ButtonController : MonoBehaviour
{
    public Texture2D cursorTexture;
    AudioSource buttonAS;
    CursorMode cursorMode = CursorMode.ForceSoftware;
    public void Awake()
    {
        buttonAS = GetComponent<AudioSource>();
    }

    public void ChangeScene(int sceneNumber)
    {
        PlayButtonClickSound();
        StartCoroutine(LoadScene(sceneNumber));
    }

    private IEnumerator LoadScene(int sceneNumber)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneNumber);
    }


    public void ChangeScene(string sceneName)
    {
        PlayButtonClickSound();
        StartCoroutine(LoadScene(sceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }


    public void ShowPointerCursor()
    {
        Cursor.SetCursor(cursorTexture, new Vector3(7f,3f,0f), cursorMode);
    }

    public void HidePointerCursor()
    {
        Cursor.SetCursor(null, Vector3.zero, cursorMode);
    }


    public void PlayButtonClickSound()
    {
        buttonAS.Play();
    }

}

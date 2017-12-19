using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[RequireComponent (typeof(AudioSource))]
public class PlayMovie : MonoBehaviour {

    public MovieTexture movie;
    public AudioSource audioSrc;

    private AudioSource gameMusicAudioSrc;
    private bool initialMusicMute;
    void Awake()
    {
        GameObject gameMusic = GameObject.FindGameObjectWithTag("Game Music");
        if (gameMusic != null)
        {
            gameMusicAudioSrc = gameMusic.GetComponent<AudioSource>();
            initialMusicMute = gameMusicAudioSrc.mute;
            gameMusicAudioSrc.mute = true;
        }
    }
    void Start()
    {
        audioSrc.clip = movie.audioClip;
        movie.Play();
        audioSrc.Play();
        StartCoroutine(WaitForMovieEnd());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            movie.Pause();
            StartCoroutine(WaitForMovieEnd());
        }
    }

    IEnumerator WaitForMovieEnd()
    {
        while (movie.isPlaying) // while the movie is playing
            yield return new WaitForEndOfFrame();

        // After movie is not playing / has stopped.
        onMovieEnded();
    }

    void onMovieEnded()
    {
        if (gameMusicAudioSrc != null)
            gameMusicAudioSrc.mute = initialMusicMute;

        SceneManager.LoadScene(1);
    }
}

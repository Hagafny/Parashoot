using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent (typeof(AudioSource))]
[RequireComponent (typeof(VideoPlayer))]
public class PlayMovie : MonoBehaviour {

    public VideoClip movie;
    public AudioSource audioSrc;

    private VideoPlayer videoPlayer;
    private AudioSource gameMusicAudioSrc;
    private bool initialMusicMute;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.clip = movie;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSrc);

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
        videoPlayer.Play();
        StartCoroutine(WaitForMovieEnd());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            videoPlayer.Pause();
            StartCoroutine(WaitForMovieEnd());
        }
    }

    IEnumerator WaitForMovieEnd()
    {
        while (videoPlayer.isPlaying)
            yield return new WaitForEndOfFrame();

        onMovieEnded();
    }

    void onMovieEnded()
    {
        if (gameMusicAudioSrc != null)
            gameMusicAudioSrc.mute = initialMusicMute;

        SceneManager.LoadScene(1);
    }
}

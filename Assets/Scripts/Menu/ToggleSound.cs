using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ToggleSound : MonoBehaviour
{
    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;
    public Image speakerImg;
    AudioSource mainAS;
    private bool _playingMusic = true;

    public void Start()
    {
        mainAS = GameObject.FindGameObjectWithTag("Game Music").GetComponent<AudioSource>();
        _playingMusic = mainAS.isPlaying;
        Toggle();
    }
    public void ToggleMusic()
    {
        _playingMusic = !_playingMusic;
        Toggle();
    }


    private void Toggle()
    {
        ToggleSprite(speakerImg);
        ToggleAudio();
    }
    private void ToggleSprite(Image speakerImg)
    {
        speakerImg.sprite = _playingMusic ? SoundOnSprite : SoundOffSprite;
    }
    private void ToggleAudio()
    {
        if (_playingMusic)
            mainAS.UnPause();
        else
            mainAS.Pause();
    }
}

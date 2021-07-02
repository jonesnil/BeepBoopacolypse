using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This is just a singleton that plays the music and doesn't stop on restarting the game.

public class Music : MonoBehaviour
{
    static Music instance;
    AudioSource music;

    void Start()
    {
        music = this.GetComponent<AudioSource>();

        DontDestroyOnLoad(this);

        if (instance == null)
        { 
            GameEvents.MusicToggle += OnMusicToggle;
            instance = this;
        }
        else
            Destroy(gameObject);

    }

    void OnMusicToggle(object sender, EventArgs args) 
    {
        if (music.isPlaying)
        {
            music.Stop();
        }
        else 
        {
            music.Play();
        }
    }
}

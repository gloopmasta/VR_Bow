using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private MusicLibrary playlist;
    private int currentIndex;

    private void Start()
    {
        source = GetComponent<AudioSource>();

        if (playlist != null )
        {
            playlist.Shuffle();
            source.clip = playlist.songList[0].clip; //the audioClip attached to the song in the songList
            currentIndex = playlist.songList.Count;
        }
    }
    private void Update()
    {
        if (!source.isPlaying)
        {
            PlayNextSong();
        }
    }

    private void PlayCurrentSong()
    {
        source.clip = playlist.songList[currentIndex].clip;
        source.Play();
    }

    private void PlayNextSong()
    {
        currentIndex++;
        if (currentIndex >= playlist.songList.Count)
        {
            currentIndex = 0; // Loop back to the first song
        }
        PlayCurrentSong();
    }
}



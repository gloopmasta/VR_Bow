using System.Globalization;
using UnityEngine;

[System.Serializable]
public class Song
{
    public AudioClip clip;
    public string songName;
    public string artistName;
    public float bpm;
    public float volume;
    public float offset;

    public Song(AudioClip mClip, string mSongName, string mSongArtist)
    {
        this.clip = mClip;
        this.songName = mSongName;
        this.artistName = mSongArtist;
        this.bpm = UniBpmAnalyzer.AnalyzeBpm(mClip);
        this.volume = 1;
        this.offset = 0;
    }
    public Song(AudioClip mClip)
    {
        this.clip = mClip;
        this.songName = mClip.name;
        this.artistName = string.Empty;
        this.bpm = UniBpmAnalyzer.AnalyzeBpm(mClip);
        this.volume = 1;
        this.offset = 0;
    }
    public Song()
    {
        this.clip = null;
        this.songName = string.Empty;
        this.artistName = string.Empty;
        this.bpm = 0;
        this.volume = 1;
        this.offset = 0;
    }


}

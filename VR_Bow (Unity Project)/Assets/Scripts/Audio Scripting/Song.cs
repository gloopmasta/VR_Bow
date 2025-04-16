using UnityEngine;

[System.Serializable]
public class Song
{
    public AudioClip clip;
    public string songName;
    public string ArtistName;
    public float bpm;
    public float volume;
    public float offset;

    public Song(AudioClip mClip, string mSongName, string mSongArtist)
    {
        this.clip = mClip;
        this.songName = mSongName;
        this.ArtistName = mSongArtist;
        this.bpm = UniBpmAnalyzer.AnalyzeBpm(mClip);
        this.volume = 1;
        this.offset = 0;
    }
    public Song(AudioClip mClip)
    {
        this.clip = mClip;
        this.songName = mClip.name;
        this.ArtistName = string.Empty;
        this.bpm = UniBpmAnalyzer.AnalyzeBpm(mClip);
        this.volume = 1;
        this.offset = 0;
    }


}

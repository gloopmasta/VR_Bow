using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Libraries/Music Library")]
public class MusicLibrary : ScriptableObject
{
    public List<Song> songList;

    public void Shuffle()
    {
        //Fisher-yates Shuffle
        for (int i = songList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1); //random number from the list
            Song temp = songList[i]; //last number in the array
            songList[i] = songList[j]; //the last non-shuffled number gets replaced by the random number J
            songList[j] = temp;  //the random number now gets the value of the last non-shuffled number in the list, completing the "swap"
        }
    }
}

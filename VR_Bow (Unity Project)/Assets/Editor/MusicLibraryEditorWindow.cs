using UnityEngine;
using UnityEditor;

public class MusicLibraryEditorWindow : EditorWindow
{
    private MusicLibrary musicLibrary;
    private Vector2 scrollPos;

    [MenuItem("Tools/Music Library Editor")]
    public static void ShowWindow()
    {
        GetWindow<MusicLibraryEditorWindow>("Music Library Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Music Library Editor", EditorStyles.boldLabel);

        // Select MusicLibrary asset
        musicLibrary = (MusicLibrary)EditorGUILayout.ObjectField("Music Library", musicLibrary, typeof(MusicLibrary), false);

        if (musicLibrary == null)
        {
            EditorGUILayout.HelpBox("Please assign a MusicLibrary asset.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < musicLibrary.songList.Count; i++)
        {
            var song = musicLibrary.songList[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Song {i + 1}", EditorStyles.boldLabel);

            song.songName = EditorGUILayout.TextField("Song Name", song.songName);
            song.artistName = EditorGUILayout.TextField("Artist Name", song.artistName);
            song.clip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", song.clip, typeof(AudioClip), false);
            song.bpm = EditorGUILayout.FloatField("BPM", song.bpm);
            song.offset = EditorGUILayout.FloatField("Offset", song.offset);

            if (GUILayout.Button("Analyze BPM"))
            {
                if (song.clip != null)
                {
                    song.bpm = UniBpmAnalyzer.AnalyzeBpm(song.clip);
                    EditorUtility.SetDirty(musicLibrary);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    Debug.LogWarning("Please assign an AudioClip before analyzing BPM.");
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add New Song"))
        {
            musicLibrary.songList.Add(new Song());
            EditorUtility.SetDirty(musicLibrary);
            AssetDatabase.SaveAssets();
        }
    }
}


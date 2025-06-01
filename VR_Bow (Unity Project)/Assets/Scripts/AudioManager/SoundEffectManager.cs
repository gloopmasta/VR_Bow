using UnityEngine;
using System.Collections.Generic;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private int audioPoolSize = 10;
    [SerializeField] private SoundEffectLibrary library;

    private List<AudioSource> audioPool = new List<AudioSource>();
    private Queue<AudioSource> availableSources = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize the library if it exists
        if (library != null)
            library.Init();

        // Create the audio pool
        CreateAudioPool();

    }

    private void Start()
    {
        //Preload clips
        PreloadAllClips();
    }

    private void CreateAudioPool()
    {
        for (int i = 0; i < audioPoolSize; i++)
        {
            CreatePooledAudioSource();
        }
    }

    private AudioSource CreatePooledAudioSource()
    {
        GameObject audioObj = new GameObject("AudioPoolObject");
        audioObj.transform.SetParent(transform);
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.spatialBlend = 1f; // 3D sound
        source.playOnAwake = false;
        audioPool.Add(source);
        availableSources.Enqueue(source);
        return source;
    }

    /// <summary>
    /// Play a sound effect by AudioCue reference
    /// </summary>
    public void Play(AudioCue cue)
    {
        if (cue == null) return;

        AudioClip clip = cue.GetRandomClip();
        if (clip == null) return;

        // Get an available source from the pool
        AudioSource source = GetAvailableAudioSource();
        if (source == null) return;

        ConfigureAudioSource(source, cue, clip);
        source.Play();
        ReturnToPoolAfterPlay(source, clip.length);
    }

    /// <summary>
    /// Play a sound effect by name, using the SoundEffectLibrary
    /// </summary>
    public void Play(string cueName)
    {
        if (library == null) return;

        AudioCue cue = library.GetCueByName(cueName);
        Play(cue);
    }

    /// <summary>
    /// Play a 3D sound effect by AudioCue reference
    /// </summary>
    public GameObject Play3D(AudioCue cue, Vector3 position, Transform parent = null, bool loop = false)
    {
        if (cue == null) return null;

        AudioClip clip = cue.GetRandomClip();
        if (clip == null) return null;

        // Get an available source from the pool
        AudioSource source = GetAvailableAudioSource();
        if (source == null) return null;

        // Position the audio source
        source.transform.position = position;
        if (parent != null) source.transform.SetParent(parent);

        ConfigureAudioSource(source, cue, clip);
        source.loop = loop;
        source.Play();

        if (!loop)
        {
            ReturnToPoolAfterPlay(source, clip.length);
        }

        return source.gameObject;
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Try to get from available queue first
        if (availableSources.Count > 0)
        {
            return availableSources.Dequeue();
        }

        // If none available, create a new one (optional - could also increase pool size)
        Debug.LogWarning("Audio pool exhausted, creating new AudioSource");
        return CreatePooledAudioSource();
    }

    private void ConfigureAudioSource(AudioSource source, AudioCue cue, AudioClip clip)
    {
        source.clip = clip;
        source.volume = cue.volume;
        source.pitch = cue.pitch;
        source.minDistance = cue.minDistance;
        source.maxDistance = cue.maxDistance;
        source.rolloffMode = cue.rolloffMode;
    }

    private void ReturnToPoolAfterPlay(AudioSource source, float duration)
    {
        StartCoroutine(ReturnToPoolCoroutine(source, duration));
    }

    private System.Collections.IEnumerator ReturnToPoolCoroutine(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset and return to pool
        source.Stop();
        source.clip = null;
        source.transform.SetParent(transform);
        availableSources.Enqueue(source);
    }

    // Optional: Preload all audio clips from the library
    public void PreloadAllClips()
    {
        if (library == null) return;

        foreach (var cue in library.GetAllCues())
        {
            foreach (var clip in cue.clips)
            {
                if (clip != null && clip.loadState != AudioDataLoadState.Loaded)
                {
                    clip.LoadAudioData();
                }
            }
        }
    }
}
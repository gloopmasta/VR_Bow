using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private SoundEffectLibrary library;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (library != null)
            library.Init();
    }

    /// <summary>
    /// Play a sound effect by AudioCue reference.
    /// </summary>
    public void Play(AudioCue cue)
    {
        if (cue == null) return;

        AudioClip clip = cue.GetRandomClip();
        if (clip == null) return;

        sfxSource.pitch = cue.pitch;
        sfxSource.PlayOneShot(clip, cue.volume);
    }

    /// <summary>
    /// Play a sound effect by name, using the SoundEffectLibrary.
    /// </summary>
    public void Play(string cueName)
    {
        if (library == null) return;

        AudioCue cue = library.GetCueByName(cueName);
        Play(cue);
    }

    /// <summary>
    /// Play a 3D sound effect by AudioCue reference.
    /// </summary>
    public GameObject Play3D(AudioCue cue, Vector3 position, Transform parent = null, bool loop = false)
    {
        if (cue == null) return null;

        AudioClip clip = cue.GetRandomClip();
        if (clip == null) return null;

        GameObject audioObj = new GameObject("Audio3D_" + cue.name);
        if (parent != null) audioObj.transform.SetParent(parent);
        audioObj.transform.position = position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = cue.volume;
        source.pitch = cue.pitch;
        source.loop = loop;

        // Apply 3D sound settings from the AudioCue
        source.minDistance = cue.minDistance;
        source.maxDistance = cue.maxDistance;
        source.rolloffMode = cue.rolloffMode;

        source.Play();

        if (!loop)
            Destroy(audioObj, clip.length + 0.5f);

        return audioObj;
    }

}

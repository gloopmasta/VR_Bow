using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioCue")]
public class AudioCue : ScriptableObject
{
    public AudioClip[] clips;
    [Range(0f, 10f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;

    public float minDistance = 1f;
    public float maxDistance = 50f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}

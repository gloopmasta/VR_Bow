using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundEffectLibrary")]
public class SoundEffectLibrary : ScriptableObject
{
    [Tooltip("List of AudioCue assets to manage all your sound effects.")]
    [SerializeField] private List<AudioCue> audioCues;

    private Dictionary<string, AudioCue> lookup;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        lookup = new Dictionary<string, AudioCue>();
        foreach (var cue in audioCues)
        {
            if (cue != null && !lookup.ContainsKey(cue.name))
                lookup.Add(cue.name, cue);
        }
    }

    /// <summary>
    /// Get an AudioCue by its name (string). Returns null if not found.
    /// </summary>
    public AudioCue GetCueByName(string name)
    {
        if (lookup == null) Init();
        return lookup.TryGetValue(name, out var cue) ? cue : null;
    }

    /// <summary>
    /// Gets all audioCues in SoundEffectLibrary
    /// </summary>
    public List<AudioCue> GetAllCues()
    {
        return audioCues;
    }
}

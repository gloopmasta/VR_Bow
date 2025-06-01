using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowSoundEffects : MonoBehaviour
{
    [SerializeField] private BowEventsSO bowEvents;

    [Header("SFX")]
    [SerializeField] private AudioCue chargeCue;
    [SerializeField] private AudioCue shootCue;

    private float chargeDiff = 0f;
    private float newCharge = 0f;
    private float oldCharge = 0f;

    private void OnEnable()
    {
        bowEvents.OnArrowReleased += () => SoundEffectManager.Instance.Play3D(shootCue, transform.position);
        //BowVisualEvents.OnChargeLevelChanged += (float nothingValue) => SoundEffectManager.Instance.Play3D(chargeCue, transform.position);
    }

    private void OnDisable()
    {
        bowEvents.OnArrowReleased -= () => SoundEffectManager.Instance.Play3D(shootCue, transform.position);
        //BowVisualEvents.OnChargeLevelChanged += (float nothingValue) => SoundEffectManager.Instance.Play3D(chargeCue, transform.position);
    }

    void HandleChargeChange(float charge)
    {


    }
}

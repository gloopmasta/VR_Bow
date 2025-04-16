using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    [SerializeField] float beatPause = 2;
    [SerializeField] float pulseSize = 1.15f;
    [SerializeField] float returnSpeed = 5f;
    private Vector3 startSize;

    private float counter;

    private void OnEnable()
    {
        counter = beatPause;
        startSize = transform.localScale;
        BeatManager.OnBeatChange += PulseOnBeat;
    }
    private void OnDisable()
    {
        transform.localScale = startSize;
        BeatManager.OnBeatChange -= PulseOnBeat;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, startSize, Time.deltaTime * returnSpeed);
    }

    void PulseOnBeat()
    {
        if (counter <= 1)
        {
            transform.localScale = startSize * pulseSize;
            counter = beatPause;
        }
        else
        {
            counter--;
        }

        return;
    }
}

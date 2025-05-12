using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeDebugger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] DriveControls player;

    private void Update()
    {
        text.text = $"Time Scale: {player.timeScale}";
    }
}

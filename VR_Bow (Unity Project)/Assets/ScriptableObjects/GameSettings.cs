using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Libraries/Game Settings")]
public class GameSettings : ScriptableObject
{
    public bool useBowController = true;

    public bool leftHanded = false;

    public string comPort;
}

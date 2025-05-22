using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PandaBT;

public class LevelFinish : MonoBehaviour
{
    [SerializeField] private LevelEventsSO levelEvents;

    [PandaTask]
    public void EndLevel()
    {
        levelEvents.RaiseLevelOneWin();
        PandaTask.Succeed();
    }
}

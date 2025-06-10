using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Other ScriptableObjects/Level Events")]
public class LevelEventsSO : ScriptableObject
{

    public event Action OnLevelOneStart;
    public event Action OnLevelOneLose;
    public event Action OnLevelOneWin;
    public event Action OnLevelOneRestart;

    public void RaiseLevelOneStart() => OnLevelOneStart?.Invoke();
    public void RaiseLevelOneLose() => OnLevelOneLose?.Invoke();
    public void RaiseLevelOneWin() => OnLevelOneWin?.Invoke();
    public void RaiseLevelOneRestart() => OnLevelOneRestart?.Invoke();

}

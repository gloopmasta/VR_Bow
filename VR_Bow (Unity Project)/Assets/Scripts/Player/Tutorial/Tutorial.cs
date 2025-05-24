using Cysharp.Threading.Tasks;
using PandaBT;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public bool steeringActivated = false;
    public bool steeringLearned = false;

    public bool jumpingActivated = false;
    public bool jumpingLearned = false;

    public bool shootingActivated = false;
    public bool shootingLearned = false;

    [PandaTask] public async Task<bool> WaitUntilSteerMessage()
    {
        await UniTask.WaitUntil(() => steeringActivated);
        return true;
    }
    [PandaTask] public async Task<bool> WaitUntilSteerLearned()
    {
        await UniTask.WaitUntil(() => steeringLearned);
        return true;
    }

    [PandaTask] public async Task<bool> WaitUntilJumpMessage()
    {
        await UniTask.WaitUntil(() => jumpingActivated);
        return true;
    }
    [PandaTask]public async Task<bool> WaitUntilJumpLearned()
    {
        await UniTask.WaitUntil(() => jumpingLearned);
        return true;
    }

    [PandaTask] public async Task<bool> WaitUntilShootMessage()
    {
        await UniTask.WaitUntil(() => shootingActivated);
        return true;
    }
    [PandaTask] public async Task<bool> WaitUntilShootLearned()
    {
        await UniTask.WaitUntil(() => shootingLearned);
        return true;
    }
}

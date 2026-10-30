using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IPlayerRuntime
{
    int ArrowCount { get; set; }
    float SlowTime { get; set; }
    int Score { get; set; }
    bool IsDashing { get; set; }
    Vector3 RespawnPosition { get; set; }
    Vector3 RespawnRotation { get; set; }

    void ResetStats();
    void ResetPosition();
    UniTaskVoid Respawn();
}

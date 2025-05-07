using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;

public class SpecialAbilityPowerup : Powerup
{
    [Header("Powerup-Specific Settings")]
    [SerializeField] private string scriptToAdd;
    [SerializeField] private float activationTime = 7f;

    protected override void ApplyEffect(GameObject player)
    {
        switch (scriptToAdd.ToLower())
        {
            case "trail":
                // var ability = player.AddComponent<DestructiveTrail>();
                //if (ability != null)
                // DisableAfterSeconds(ability).Forget();
                break;

            case "spinninglaser":
                break;
            default:
                Debug.LogWarning("No valid ability name entered");
                break;
        }
    }

    private async UniTaskVoid DisableAfterSeconds(MonoBehaviour script)
    {
        await UniTask.WaitForSeconds(activationTime);
        //if you wanna ignore timeScale: await UniTask.Delay((int)(activationTime * 1000));
        Destroy(script);
    }
}

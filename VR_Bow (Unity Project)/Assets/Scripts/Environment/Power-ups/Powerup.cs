using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Powerup : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float timeToDestroy = 1f;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null, "No animator on Powerup");
    }

    public void Collect(GameObject player)
    {
        ApplyEffect(player);
        GetComponent<BoxCollider>().enabled = false; //disable thre powerup
        PlayAnimation();
        DestroyAfterTime().Forget(); // fire-and-forget async task
        //.Forget() is needed because UniTaskVoid can’t be awaited, and you're calling from a non-async method.
    }

    protected abstract void ApplyEffect(GameObject player);

    private void PlayAnimation()
    {
        if (animator != null)
        {
            animator.CrossFade("Powerup collect", 0);
        }
    }


    private async UniTaskVoid DestroyAfterTime()
    {
        await UniTask.WaitForSeconds(timeToDestroy);

        Destroy(gameObject);
    }


}

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Player Warning Messages")]
    public GameObject rotateBowPanel;
    public GameObject fadeScreen;

    [Header("Race UI")]
    public GameObject racePanel;
    public  TextMeshProUGUI timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public float FadeToBlack()
    //{

    //    if(!fadeScreen.GetComponent<Animator>()) { Debug.LogWarning("No animator found in fadePanel"); return 0f; }

    //    fadeScreen.SetActive(true);

    //    Animator animator = fadeScreen.GetComponent<Animator>();

    //    animator.CrossFade("FadeToBlack", 1f);

    //    return animator.GetCurrentAnimatorClipInfo(0).Length;
    //}

    //public void FadeFromBlack()
    //{

    //    if (!fadeScreen.GetComponent<Animator>()) { Debug.LogWarning("No animator found in fadePanel"); return; }



    //    Animator animator = fadeScreen.GetComponent<Animator>();

    //    animator.CrossFade("FadeOutBlack", 1f);

    //    fadeScreen.SetActive(false);
    //}

    public async UniTask FadeToBlackAsync()
    {
        Animator animator = fadeScreen.GetComponent<Animator>();

        if (!animator)
        {
            Debug.LogWarning("No animator found in fadePanel");
            return;
        }

        fadeScreen.SetActive(true);
        animator.CrossFade("FadeToBlack", 0.1f);

        // Wait until the animation starts playing
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FadeToBlack"));

        // Wait until the animation finishes
        await UniTask.WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f &&
            !animator.IsInTransition(0));
    }

    public async UniTask FadeFromBlackAsync()
    {
        Animator animator = fadeScreen.GetComponent<Animator>();

        if (!animator)
        {
            Debug.LogWarning("No animator found in fadePanel");
            return;
        }

        animator.CrossFade("FadeOutBlack", 0.1f);

        // Wait until the animation starts playing
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FadeOutBlack"));

        // Wait until the animation finishes
        await UniTask.WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f &&
            !animator.IsInTransition(0));

        fadeScreen.SetActive(false);
    }

    public void DisplaySwitchMessage()
    {
        Animator animator =  rotateBowPanel.GetComponent<Animator>();

        if (animator != null && rotateBowPanel != null)
        {
            rotateBowPanel.SetActive(true);
            animator.CrossFade("FadeIn", 0f, 1); //enable fadeIn
            animator.CrossFade("turnBow", 0f, 0); //enable turning animation
        }
    }
    public async void RemoveSwitchMessage()
    {
        Animator animator = rotateBowPanel.GetComponent<Animator>();

        if (animator != null && rotateBowPanel != null)
        {
            animator.CrossFade("FadeOut", 0f, 1); //fade out animation
            await UniTask.WaitForSeconds(1f);
            rotateBowPanel.SetActive(false);
        }
    }

}

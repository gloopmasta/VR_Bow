using Cysharp.Threading.Tasks;
using PandaBT;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Timing settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Player Warning Messages")]
    public GameObject fadeScreen;

    [PandaVariable] public GameObject calibratePanel;
    [PandaVariable] public GameObject warningPanel;
    [PandaVariable] public GameObject rotateBowPanel;
    [PandaVariable] public GameObject jumpInstruction;
    [PandaVariable] public GameObject steerInstruction;
    [PandaVariable] public GameObject shootInstruction;
    public GameObject damageIndicator;

    [Header("Race UI")]
    public GameObject racePanel;
    public  TextMeshProUGUI timer;

    [PandaTask]
    public async Task<bool> FadeIn(GameObject panel)
    {
        panel.SetActive(true); //set panel active

        CanvasGroup  canvasGroup = panel.GetComponent<CanvasGroup>(); //assign canvasgroup
        if (canvasGroup == null)
        {
            Debug.Log("no canvasGroup found in panel " + panel.name);
            return false;
        }
        
        canvasGroup.alpha = 0;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;  //fade in the alpha on cnavas group
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            await UniTask.Yield();
        }

        canvasGroup.alpha = 1f;
        return true;
    }

    public async UniTask<bool> FadeIn(GameObject panel, float duration)
    {
        panel.SetActive(true); //set panel active

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>(); //assign canvasgroup
        if (canvasGroup == null)
        {
            Debug.Log("no canvasGroup found in panel " + panel.name);
            return false;
        }

        canvasGroup.alpha = 0;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;  //fade in the alpha on cnavas group
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            await UniTask.Yield();
        }

        canvasGroup.alpha = 1f;
        return true;
    }

    [PandaTask]
    public async Task<bool> FadeOut(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.Log("no canvasGroup found in panel " + panel.name);
            return false;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsed / fadeDuration));
            await UniTask.Yield();
        }

        canvasGroup.alpha = 0f;
        panel.SetActive(false); //set panel inactive

        return true;
    }

    public async UniTask<bool> FadeOut(GameObject panel, float duration)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.Log("no canvasGroup found in panel " + panel.name);
            return false;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsed / duration));
            await UniTask.Yield();
        }

        canvasGroup.alpha = 0f;
        panel.SetActive(false); //set panel inactive

        return true;
    }


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

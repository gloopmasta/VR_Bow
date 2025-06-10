using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CalibrateButton : MonoBehaviour
{
    [SerializeField] private string unchargedText = "Hold the bow uncharged. ";
    [SerializeField] private string chargedText = "Charge the bow. ";
    [SerializeField] private int duration = 3;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow")) //if arrow shoots
        {
            Calibration().Forget();
        }

    }

    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 40, 300, 40), "trigger calibration"))
    //    {
    //        Calibration().Forget();
    //    }
    //}

    public async UniTaskVoid Calibration()
    {
        BowShooting shootingScript = GameManager.Instance.player.GetComponent<BowShooting>();
        PlayerUIManager uiManager = GameManager.Instance.player.GetComponent<PlayerUIManager>();
        TextMeshProUGUI tmpText = uiManager.calibratePanel.GetComponentInChildren<TextMeshProUGUI>();

        DisableBoxColliders();

        tmpText.text = unchargedText + duration.ToString(); //change text
        await uiManager.FadeIn(uiManager.calibratePanel); // Fade in calirate panel

        //UNCHARGED
        for (int timer = duration; timer >= 0; timer--)
        {
            tmpText.text = unchargedText + timer.ToString();
            await UniTask.WaitForSeconds(1f);
        }

        shootingScript.minCalibration = shootingScript.rawFlex;

        //CHARGED
        uiManager.calibratePanel.GetComponent<Animator>().CrossFade("calibrateToCharge", 0f);

        for (int timer = duration; timer >= 0; timer--)
        {
            tmpText.text = chargedText + timer.ToString();
            await UniTask.WaitForSeconds(1f);
        }

        shootingScript.maxCalibration = shootingScript.rawFlex;

        await uiManager.FadeOut(uiManager.calibratePanel);

        await UniTask.WaitForSeconds(2f);
        
        EnableBoxColliders(); //re-enable box colliders after 2 sec
    }

    public void DisableBoxColliders()
    {
        // Disable this object's BoxCollider
        if (TryGetComponent<BoxCollider>(out var myCollider))
        {
            myCollider.enabled = false;
        }

        // Disable all children BoxColliders of the parent
        if (transform.parent != null)
        {
            foreach (var collider in transform.parent.GetComponentsInChildren<BoxCollider>(true))
            {
                collider.enabled = false;
            }
        }
    }

    public void EnableBoxColliders()
    {
        // Enable this object's BoxCollider
        if (TryGetComponent<BoxCollider>(out var myCollider))
        {
            myCollider.enabled = true;
        }

        // Enable all children BoxColliders of the parent
        if (transform.parent != null)
        {
            foreach (var collider in transform.parent.GetComponentsInChildren<BoxCollider>(true))
            {
                collider.enabled = true;
            }
        }
    }
}

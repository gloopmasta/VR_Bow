using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableTargetPractise : MonoBehaviour
{
    [SerializeField] private GameObject targets;
    [SerializeField] private GameObject copy;

    private void OnEnable()
    {
        copy = Instantiate(targets);
        targets.SetActive(false);
        copy.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow")) //if arrow shoots
        {
            ActivateTargets();

        }

    }

    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 25, 300, 40), "trigger targets"))
    //    {
    //        ActivateTargets();
    //    }
    //}

    void ActivateTargets()
    {
        Destroy(copy);
        copy.SetActive(false);
        copy = Instantiate(targets);
        copy.SetActive(true);
    }
}

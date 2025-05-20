using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Activator : MonoBehaviour
{
    [Header("GameObjects to activate")]
    [SerializeField] private List<GameObject> objects = new List<GameObject>();
    [SerializeField] private bool isActivated = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float timeToDestroy = 1f;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null, "No animator on Activator");

        PlaySpawnAnimation();
    }
    private void OnDisable()
    {
        PlayDespawnAnimation();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            ActivateObjects();
            SetPlayerRespawnPoint(other.gameObject);
        }
    }

    private void ActivateObjects()
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null || obj.activeSelf) //if object is null or already active
                obj.SetActive(true);
        }
    }

    private void PlaySpawnAnimation()
    {
        if (animator != null)
        {
            animator.CrossFade("Powerup spawn", 0);
        }
    }

    private void PlayDespawnAnimation()
    {
        if (animator != null)
        {
            animator.CrossFade("Powerup collect", 0);
        }
    }

    private void SetPlayerRespawnPoint(GameObject player)
    {
        if (!player.GetComponent<Player>()) { Debug.Log("no player script found in player collision " + player); return; }

        player.GetComponent<Player>().respawnPosition = transform.position; //set respawnPosition to the last checkpoint
        player.GetComponent<Player>().respawnRotation = transform.rotation.eulerAngles; //set rotation to the last checkpoint
    }

}
 
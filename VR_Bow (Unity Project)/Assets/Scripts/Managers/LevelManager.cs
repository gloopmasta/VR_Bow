using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        map.SetActive(false);
}

    [Header("Start Scene")]
    public GameObject coverDome;
    public GameObject startUI;

    [Header("Level one")]
    public GameObject firstRoad;
    public GameObject firstActivator;
    public GameObject map;

    public void FinishStartUI()
    {
        startUI.SetActive(false);
        //add animation here

        firstActivator.SetActive(true);

        firstRoad.SetActive(true);
        //activate skybox
    }
}

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class Road : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Material[] instanceMaterials;
    private Renderer[] childRenderers;
    private bool materialsInitialized = false;
    private CancellationTokenSource fadeTokenSource;

    private void OnEnable() // Changed from Awake to OnEnable
    {
        if (!materialsInitialized)
        {
            InitializeMaterials();
            materialsInitialized = true;
        }

        SetAllMaterialsTransparency(0f);

        ActivateRoad().Forget();
    }

    private void InitializeMaterials()
    {
        childRenderers = GetComponentsInChildren<Renderer>(true);
        instanceMaterials = new Material[childRenderers.Length];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            instanceMaterials[i] = new Material(childRenderers[i].sharedMaterial)
            {
                enableInstancing = true
            };
            childRenderers[i].material = instanceMaterials[i];
        }
    }

    public async UniTask ActivateRoad()
    {
        fadeTokenSource?.Cancel();
        fadeTokenSource = new CancellationTokenSource();
        await FadeIn(fadeTokenSource.Token);
    }

    private async UniTask FadeIn(CancellationToken token)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            token.ThrowIfCancellationRequested();

            float progress = fadeCurve.Evaluate(elapsedTime / fadeDuration);
            SetAllMaterialsTransparency(progress);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        SetAllMaterialsTransparency(1f); // Ensure fully visible
    }

    private void SetAllMaterialsTransparency(float alpha)
    {
        foreach (var mat in instanceMaterials)
        {
            SetMaterialTransparency(mat, alpha);
        }
    }

    private void SetMaterialTransparency(Material mat, float alpha)
    {
        if (mat.HasProperty("_BaseColor"))
        {
            Color color = mat.GetColor("_BaseColor");
            color.a = alpha;
            mat.SetColor("_BaseColor", color);
        }

        // Additional URP transparency settings
        //mat.SetFloat("_Surface", alpha < 0.99f ? 1 : 0);
        //mat.SetOverrideTag("RenderType", alpha < 0.99f ? "Transparent" : "Opaque");
        //mat.renderQueue = alpha < 0.99f ? (int)UnityEngine.Rendering.RenderQueue.Transparent : -1;
    }

    private void OnDisable()
    {
        fadeTokenSource?.Cancel();
    }

    private void OnDestroy()
    {
        fadeTokenSource?.Cancel();
        if (instanceMaterials != null)
        {
            foreach (var mat in instanceMaterials)
            {
                if (mat != null) Destroy(mat);
            }
        }
    }
}

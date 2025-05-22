using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    [TextArea] public string fullText;
    public float characterDelay = 0.05f;

    private async void OnEnable()
    {
        
        await PlayTypewriterEffect();
    }

    private async UniTask PlayTypewriterEffect()
    {
        textComponent.text = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            await UniTask.Delay(System.TimeSpan.FromSeconds(characterDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}

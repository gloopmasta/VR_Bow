using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;

    public void LoadScene()
    {
        SceneManager.LoadScene("Level 1Matteo");
    }
}

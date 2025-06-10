using UnityEngine;
using System.Diagnostics;
using System.IO;

public class GameHardReset : MonoBehaviour
{
    public void HardResetGame()
    {
#if UNITY_STANDALONE
        RestartStandaloneBuild();
#else
        // Fallback for platforms that don't support process restart
        Debug.LogWarning("Hard reset not supported on this platform. Reloading scene instead.");
#endif
    }

    private void RestartStandaloneBuild()
    {
        string executablePath = GetExecutablePath();
        if (File.Exists(executablePath))
        {
            // Launch new instance
            Process.Start(executablePath);

            // Close current instance
            Application.Quit();

            // Force kill in case Quit() fails
            Process.GetCurrentProcess().Kill();
        }
        else
        {

        }
    }

    private string GetExecutablePath()
    {
        // Windows
#if UNITY_STANDALONE_WIN
        return Application.dataPath.Replace("_Data", ".exe");

        // Mac (app bundle)
#elif UNITY_STANDALONE_OSX
        return Path.Combine(
            Path.GetDirectoryName(Application.dataPath),
            Path.GetFileNameWithoutExtension(Application.dataPath) + ".app"
        );
        
        // Linux
#elif UNITY_STANDALONE_LINUX
        return Application.dataPath.Replace("_Data", ".x86_64");
#endif
    }
}

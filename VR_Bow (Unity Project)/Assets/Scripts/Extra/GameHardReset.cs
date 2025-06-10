using UnityEngine;
using System.Diagnostics;
using System.IO;

public class GameHardReset : MonoBehaviour
{
    [Tooltip("Filename of the no-intro version (e.g. 'Game_NoIntro.exe')")]
    public string noIntroBuildName = "NoIntro.exe"; // Set this in Inspector

    public void HardResetGame()
    {
#if UNITY_STANDALONE
        RestartToNoIntroBuild();
#else
        Debug.LogWarning("Hard reset not supported. Reloading scene instead.");
#endif
    }

    private void RestartToNoIntroBuild()
    {
        string noIntroPath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName, // Build folder
            noIntroBuildName
        );

        if (File.Exists(noIntroPath))
        {
            // Launch no-intro version
            Process.Start(noIntroPath);

            // Close current game
            Application.Quit();

            // Force quit if needed
            System.Threading.Thread.Sleep(1000); // Give time for new process to launch
            Process.GetCurrentProcess().Kill();
        }
        else
        {
            //Debug.LogError($"No-intro build not found at: {noIntroPath}");
            // Fallback to regular restart
            Process.Start(Application.dataPath.Replace("_Data", ".exe"));
            Application.Quit();
        }
    }


    //private void RestartStandaloneBuild()
    //{
    //    string executablePath = GetExecutablePath();
    //    if (File.Exists(executablePath))
    //    {
    //        // Launch new instance
    //        Process.Start(executablePath);

    //        // Close current instance
    //        Application.Quit();

    //        // Force kill in case Quit() fails
    //        Process.GetCurrentProcess().Kill();
    //    }
    //    else
    //    {

    //    }
    //}

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

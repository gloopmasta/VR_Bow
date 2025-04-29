using PandaBT;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class CScriptMatteoTask : MonoBehaviour
{
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
            Debug.LogWarning("No Renderer found on GameObject.");
    }

    private async void Start()
    {
        Debug.Log("Wacht op rechtermuisklik...");
        await WaitForRightClickAsync();
        Debug.Log("Rechtermuisklik gedetecteerd!");
    }


    [PandaTask]
    public async Task<bool> WaitForRightClickAsync()
    {
        while (!Mouse.current.rightButton.wasPressedThisFrame)
        {
            await Task.Yield();
        }

        return true;
    }



    [PandaTask]
    public void ChangeColor(string colorName)
    {

        Color color;

        switch (colorName.ToLower())
        {
            case "red":
                color = Color.red;
                break;
            case "green":
                color = Color.green;
                break;
            case "blue":
                color = Color.blue;
                break;
            case "yellow":
                color = Color.yellow;
                break;
            case "white":
                color = Color.white;
                break;
            case "black":
                color = Color.black;
                break;
            case "cyan":
                color = Color.cyan;
                break;
            case "magenta":
                color = Color.magenta;
                break;
            case "gray":
            case "grey":
                color = Color.gray;
                break;
            default:
                Debug.LogWarning($"Color '{colorName}' not recognized. Defaulting to black.");
                color = Color.black;
                break;
        }

        rend.material.color = color;
        PandaTask.Succeed();
    }


    [PandaTask]
    void Rotate(float IrotationSpeed)
    {
        float rotationSpeed = IrotationSpeed;
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }      
}

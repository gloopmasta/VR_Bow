using UnityEngine;
using System.IO.Ports;

public class PotentiometerReader : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM13", 115200); // Replace with your actual port
    string serialInput = "";
    [Range(0, 1)] public float sensorValue = 0f;
    //public float movementSpeed = 0f;
    //public float maxSpeed = 100f;
    //public float accelerationRate = 2;

    void Start()
    {
        serialPort.Open();
        serialPort.ReadTimeout = 50;
    }

    void Update()
    {
        if (serialPort.IsOpen)
        {
            try
            {
                // Read until newline
                string line = serialPort.ReadLine();
                
                if (int.TryParse(line, out int rawValue))
                {
                    sensorValue = Mathf.Clamp01(rawValue / 4095f); // Normalize to 0–1
                    Debug.Log($"Pot Value: {sensorValue}");
                }
            }
            catch (System.Exception)
            {
                // Ignore timeout or invalid input
            }
        }

        //movementSpeed = maxSpeed * Mathf.Pow((sensorValue -0.5f), accelerationRate); // Exponential scaling
        //transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
    }

    void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
            serialPort.Close();
    }
}

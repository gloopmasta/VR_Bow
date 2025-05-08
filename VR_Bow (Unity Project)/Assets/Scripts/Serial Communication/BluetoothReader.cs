using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class BluetoothReader : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM13", 115200); // Replace "COM3" with your HC-05 COM port
    Thread readThread;
    bool isRunning = false;
    public float sensorValue = 0f;
    string receivedData = "";

    void Start()
    {
        serialPort.Open();
        isRunning = true;
        readThread = new Thread(ReadSerial);
        readThread.Start();
    }

    void ReadSerial()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                receivedData = serialPort.ReadLine();
                Debug.Log("Received: " + receivedData);

                // Read until newline
                string line = serialPort.ReadLine();

                if (int.TryParse(line, out int rawValue))
                {
                    sensorValue = Mathf.Clamp01(rawValue / 4095f); // Normalize to 0–1
                    Debug.Log($"Pot Value: {sensorValue}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading serial data: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
            readThread.Join();
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}

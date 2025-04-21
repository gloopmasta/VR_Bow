using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class BluetoothReader : MonoBehaviour
{
    SerialPort serialPort = new SerialPort("COM6", 9600); // Replace "COM3" with your HC-05 COM port
    Thread readThread;
    bool isRunning = false;
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

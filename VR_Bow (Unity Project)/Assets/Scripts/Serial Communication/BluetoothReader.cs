using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using System;

public class BluetoothReader : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    public string comPort;
    SerialPort serialPort;
    Thread readThread;
    volatile bool isRunning = false;
    ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
    public float sensorValue = 0f;

    void OnEnable()
    {
        comPort = gameSettings.comPort;
        try
        {
            serialPort = new SerialPort(comPort, 115200);
            serialPort.ReadTimeout = 100; // Set a read timeout to prevent blocking
            serialPort.Open();
            isRunning = true;
            readThread = new Thread(ReadSerial);
            readThread.Start();
            Debug.Log("Serial port opened and read thread started.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to open serial port: " + e.Message);
        }
    }

    void ReadSerial()
    {
        while (isRunning)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    string line = serialPort.ReadLine();
                    dataQueue.Enqueue(line);
                }
            }
            catch (TimeoutException)
            {
                // Ignore timeout exceptions and continue reading
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading from serial port: " + e.Message);
            }
        }
    }

    void Update()
    {
        while (dataQueue.TryDequeue(out string line))
        {
            if (int.TryParse(line, out int rawValue))
            {
                sensorValue = Mathf.Clamp01(rawValue / 2900f);
                //Debug.Log($"Received Analog Value: {rawValue}, Normalized: {sensorValue}");
            }
            else
            {
                Debug.LogWarning("Received malformed data: " + line);
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }

        if (serialPort != null)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.Dispose();
        }

        Debug.Log("Serial port closed and resources cleaned up.");
    }
}

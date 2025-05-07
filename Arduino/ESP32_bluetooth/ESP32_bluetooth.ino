#include "BluetoothSerial.h"

BluetoothSerial SerialBT;
const int analogPin = 34; // GPIO34 for analog input

void setup() {
  Serial.begin(115200);
  SerialBT.begin("ESP32_BT_Test"); // Bluetooth device name
  Serial.println("Bluetooth device is ready to pair");
}

void loop() {
  int analogValue = analogRead(analogPin);
  float voltage = analogValue * (3.3 / 4095.0); // Convert to voltage (ESP32 has 12-bit ADC)
  
  // Send data via Bluetooth
  SerialBT.print("Analog Value: ");
  SerialBT.print(analogValue);
  SerialBT.print(", Voltage: ");
  SerialBT.println(voltage, 2); // 2 decimal places
  
  // Also print to serial for debugging
  Serial.print("Sent via BT: ");
  Serial.print(analogValue);
  Serial.print(", ");
  Serial.println(voltage, 2);
  
  delay(500); // Send every 500ms
}

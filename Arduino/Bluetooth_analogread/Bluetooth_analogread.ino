#include "BluetoothSerial.h"

BluetoothSerial SerialBT;
const int analogPin = 34; // GPIO34 for analog input

void setup() {
  Serial.begin(115200);
  SerialBT.begin("ESP32_Bow_Controller"); 
}

void loop() {
  SerialBT.println(analogRead(analogPin));
  
delay(5);
}

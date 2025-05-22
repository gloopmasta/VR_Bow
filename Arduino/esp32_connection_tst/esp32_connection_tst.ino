#include "BluetoothSerial.h"

BluetoothSerial SerialBT;

void setup() {
  Serial.begin(115200);
  SerialBT.begin("ESP32_Lithium test"); 
}

void loop() {
  SerialBT.println("bluetooth is still working");
  
delay(5);
}

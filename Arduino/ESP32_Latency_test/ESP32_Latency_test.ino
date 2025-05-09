#include "BluetoothSerial.h"

BluetoothSerial SerialBT;
unsigned long sendTime = 0;
bool waitingForReply = false;
int testCount = 0;
const int totalTests = 100;  // Number of tests to run

void setup() {
  Serial.begin(115200);
  SerialBT.begin("ESP32_Latency_Test");  // Bluetooth device name
  Serial.println("Bluetooth started. Pair with ESP32_Latency_Test!");
}

void loop() {
  if (!waitingForReply && testCount < totalTests) {
    sendTime = micros();  // Record send time (in microseconds)
    SerialBT.println(sendTime);  // Send timestamp to PC
    waitingForReply = true;
    Serial.print("Sent: "); Serial.println(sendTime);
    delay(10);  // Small delay to avoid flooding
  }

  if (waitingForReply && SerialBT.available()) {
    unsigned long receivedTime = SerialBT.parseInt();  // Read echoed timestamp
    unsigned long roundTripTime = micros() - receivedTime;  // Calculate RTT
    Serial.print("Latency: "); Serial.print(roundTripTime); Serial.println(" µs");
    waitingForReply = false;
    testCount++;
  }

  if (testCount >= totalTests) {
    Serial.println("Latency test complete!");
    delay(10000);  // Stop after tests
  }
}

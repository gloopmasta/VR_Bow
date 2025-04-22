#include <SPI.h>
#include <RF24.h>

RF24 radio(9, 10); // CE, CSN
const byte address[6] = "00001";

void setup() {
  Serial.begin(9600);
  radio.begin();
  radio.openWritingPipe(address);
  radio.openReadingPipe(1, address);
  radio.setPALevel(RF24_PA_LOW);
  radio.stopListening();
}

void loop() {
  unsigned long startTime = millis();
  radio.stopListening();
  radio.write(&startTime, sizeof(startTime)); // Send current time
  radio.startListening();

  unsigned long startWait = millis();
  bool timeout = false;
  while (!radio.available()) {
    if (millis() - startWait > 200) { // 200ms timeout
      timeout = true;
      break;
    }
  }

  if (!timeout) {
    unsigned long receivedTime;
    radio.read(&receivedTime, sizeof(receivedTime));
    unsigned long rtt = millis() - receivedTime;
    Serial.print("Round-trip delay: ");
    Serial.print(rtt);
    Serial.println(" ms");
  } else {
    Serial.println("No response received.");
  }

  delay(1); // Wait before next measurement
}

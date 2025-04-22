#include <SPI.h>
#include <RF24.h>

RF24 radio(9, 10); // CE, CSN
const byte address[6] = "00001";

void setup() {
  radio.begin();
  radio.openReadingPipe(1, address);
  radio.openWritingPipe(address);
  radio.setPALevel(RF24_PA_LOW);
  radio.startListening();
}

void loop() {
  if (radio.available()) {
    unsigned long receivedTime;
    radio.read(&receivedTime, sizeof(receivedTime));
    radio.stopListening();
    radio.write(&receivedTime, sizeof(receivedTime)); // Send back the received time
    radio.startListening();
  }
}

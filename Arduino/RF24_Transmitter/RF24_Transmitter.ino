#include <SPI.h>
#include <RF24.h>

RF24 radio(9, 10); // CE, CSN
const byte address[6] = "00001";

void setup() {
  radio.begin();
  radio.openWritingPipe(address);
  radio.setPALevel(RF24_PA_LOW);
  radio.stopListening();
}

void loop() {
  int potValue = analogRead(A0); // Read potentiometer value
  radio.write(&potValue, sizeof(potValue)); // pointer adress of potvalue (&) and the size of potvalue in bytes
  delay(0); // Wait for half a second
}

#include <SPI.h>
#include <RF24.h>

RF24 radio(9, 10); // CE, CSN
const byte address[6] = "00001";

void setup() {
  Serial.begin(9600);
  radio.begin();
  radio.openReadingPipe(0, address);
  radio.setPALevel(RF24_PA_LOW);
  radio.startListening();
}

void loop() {
  if (radio.available()) {
    int potValue;
    int delayValue;
    radio.read(&potValue, sizeof(potValue));
    radio.read(&delayValue, sizeof(delayValue));

    Serial.println(potValue);
  }
}

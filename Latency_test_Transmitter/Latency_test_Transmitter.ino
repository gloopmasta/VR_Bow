#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>

RF24 radio(9, 10); // CE, CSN
const byte address[6] = "00001";

unsigned long start_time;
unsigned long round_trip_time;

void setup() {
  Serial.begin(9600);
  radio.begin();
  radio.openWritingPipe(address);
  radio.openReadingPipe(1, address);
  radio.setPALevel(RF24_PA_LOW);
  radio.stopListening();
}

void loop() {
  start_time = micros();
  radio.write(&start_time, sizeof(start_time)); // Send timestamp

  radio.startListening();

  unsigned long timeout = micros() + 100000; // 100ms timeout
  while (!radio.available()) {
    if (micros() > timeout) {
      Serial.println("Timeout");
      return;
    }
  }

  unsigned long received_time;
  radio.read(&received_time, sizeof(received_time));
  round_trip_time = micros() - start_time;

  Serial.print("RTT (us): ");
  Serial.println(round_trip_time);
  delay(1000); // Wait before next ping
}

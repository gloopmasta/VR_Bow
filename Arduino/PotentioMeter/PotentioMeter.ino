int potVal = 0;
float adjustedVal;

void setup(){
  Serial.begin(9600); 
}


void loop() {
potVal = analogRead(A0);

adjustedVal = (float)potVal / 1023;
 
Serial.println(potVal);
delay(5);
}

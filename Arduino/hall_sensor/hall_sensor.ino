void setup() {
  Serial.begin(115200); // Start serial at safe speed
  pinMode(34, INPUT); // Not strictly necessary but good practice
  Serial.println("Hall Sensor Test - Safe Mode");
}

void loop() {
  int sensorValue = analogRead(34); // Read from A0
  float voltage = sensorValue * (5.0 / 1023.0); // Convert to voltage (5V reference)
  
  Serial.print("Raw: ");
  Serial.print(sensorValue);
  Serial.print("\tVoltage: ");
  Serial.print(voltage, 3); // 3 decimal places
  Serial.println("V");
  
  delay(200); // Slow enough to read safely
}

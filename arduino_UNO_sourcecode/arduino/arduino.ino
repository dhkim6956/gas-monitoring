int GasPin = A0;


void setup() {
  pinMode(GasPin,INPUT);
  Serial.begin(9600);
}

void loop() {
  char buf[30];
  int AD = analogRead(GasPin);

  sprintf(buf,"AT+Temp=%d\r\n",AD);
  Serial.print(buf);
  delay(1000);
}

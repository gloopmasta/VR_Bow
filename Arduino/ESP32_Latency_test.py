# Install PySerial if needed: pip install pyserial
import serial
import serial.tools.list_ports

# Find the ESP32's COM port (Windows)
def find_esp32_port():
    ports = serial.tools.list_ports.comports()
    for port in ports:
        if "CP2102" in port.description or "Silicon Labs" in port.description:
            return port.device
    return None

port_name = find_esp32_port()
if not port_name:
    print("ESP32 not found! Check Bluetooth pairing.")
    exit()

# Configure serial connection
ser = serial.Serial(port_name, 115200, timeout=1)
print(f"Connected to {port_name}. Waiting for data...")

while True:
    if ser.in_waiting:
        timestamp = ser.readline().decode().strip()  # Read ESP32's timestamp
        ser.write((timestamp + "\n").encode())      # Echo it back
        print(f"Echoed: {timestamp}")

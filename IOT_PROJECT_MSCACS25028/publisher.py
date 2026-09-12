import paho.mqtt.client as mqtt
import json
import time
import random

# =========================
# CONFIGURATION
# =========================
BROKER = "localhost"  
PORT = 1883

TOPIC_PUB = "home_status"
TOPIC_SUB = "home_control"

DEVICE_ID = "raspberry_01"

# =========================
# MQTT CALLBACKS
# =========================

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("✅ Connected to MQTT Broker")
        client.subscribe(TOPIC_SUB)
    else:
        print(f"❌ Connection failed with code {rc}")

def on_message(client, userdata, msg):
    command = msg.payload.decode()
    print(f"📩 Received command: {command}")

    if command == "HEATING_ON":
        print("🔥 Heating system ON")
    elif command == "HEATING_OFF":
        print("❄️ Heating system OFF")

# =========================
# DATA GENERATION
# =========================

def generate_data():
    return {
        "device_id": DEVICE_ID,
        "temperature": round(random.uniform(18.0, 26.0), 2),
        "temperature_unit": "C",
        "humidity": random.randint(40, 70),
        "humidity_unit": "%",
        "door": random.choice(["open", "closed"]),
        "window": random.choice(["open", "closed"])
    }

# =========================
# MAIN
# =========================

def main():
    client = mqtt.Client()

    client.on_connect = on_connect
    client.on_message = on_message

    try:
        print("🔌 Connecting to broker...")
        client.connect(BROKER, PORT, 60)
    except Exception as e:
        print(f"❌ Connection error: {e}")
        return

    client.loop_start()

    try:
        while True:
            data = generate_data()
            message = json.dumps(data)

            client.publish("home_status", message)
            print(f"📤 Published: {message}")

            time.sleep(5)

    except KeyboardInterrupt:
        print("\n🛑 Stopped by user")

    finally:
        client.loop_stop()
        client.disconnect()

# =========================

if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""
Base file credits goes to:
Albert Huang <albert@csail.mit.edu> PyBluez simple example rfcomm-server.py
"""

import bluetooth
import os
import logging
import time
import json
import fileinput

logging.basicConfig(
    level=logging.DEBUG,
    filename="/tmp/bt_server.log",
    filemode="w",
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)


def update_time(unix_time_as_string):
    clk_id = time.CLOCK_REALTIME
    time.clock_settime(clk_id, float(unix_time_as_string))


def update_sunrise_set(sunset: str, sunrise: str):
    path = "/home/pi/.openauto/config/openauto_system.ini"
    ini = ""

    with open(path, "r") as fr:
        for line in fr:
            if "SunriseTime=" in line:
                line = f"SunriseTime={sunrise}\n"
            elif "SunsetTime=" in line:
                line = f"SunsetTime={sunset}\n"

            ini = ini + line

    with open(path, "w") as fw:
        fw.write(ini)


def update_temperature(temperature: str):
    sensor_value = f"t={float(temperature) * 1000}"
    sensor_file = "/home/pi/oap_bt/sensor"

    with open(sensor_file, "w") as f:
        f.write(sensor_value)


os.system("sudo hciconfig hci0 piscan")
os.system("sudo chmod 777 /var/run/sdp")

logging.info("os system done")

server_sock = bluetooth.BluetoothSocket(bluetooth.RFCOMM)
server_sock.bind(("", 22))
server_sock.listen(1)

port = server_sock.getsockname()[1]

uuid = "94f39d29-7d6d-437d-973b-fba39e49d4ee"

bluetooth.advertise_service(
    server_sock,
    "OAT_BT_Server",
    service_id=uuid,
    service_classes=[uuid, bluetooth.SERIAL_PORT_CLASS],
    profiles=[bluetooth.SERIAL_PORT_PROFILE],
)

while True:
    logging.info(f"Waiting for connection on RFCOMM channel {port}")

    client_sock, client_info = server_sock.accept()
    logging.info(f"Accepted connection from {client_info}")
    synced = {"time": False, "suntime": False}

    try:
        while True:
            data = client_sock.recv(1024).decode("utf8")
            logging.info(f"Received {data}")
            if not data:
                break

            data = json.loads(data)

            update_time(data["Time"])
            synced["time"] = True

            if data["Sunset"] and data["Sunrise"]:
                update_sunrise_set(data["Sunset"], data["Sunrise"])
                synced["suntime"] = True

            if data["Temperature"]:
                update_temperature(data["Temperature"])
                synced["temperature"] = True

            client_sock.send(json.dumps(synced).encode("utf8"))
    except OSError as ex:
        logging.error(ex)
        logging.info("Disconnected")
    finally:
        client_sock.close()

server_sock.close()

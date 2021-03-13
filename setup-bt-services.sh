#!/bin/bash
//This script will setup everything for you in automated session, jsut run it

cd ~
echo "Add -c option at the end of ExecStart parameter"
sudo sed -i '/^ExecStart/ s/$/ -C/' /etc/systemd/system/dbus-org.bluez.service
echo "Add ExecStartPost=usr/bin/sdptool add SP line after ExecStart"
sudo sed -i '/^ExecStart.*/a ExecStartPost=/usr/bin/sdptool add SP' /etc/systemd/system/dbus-org.bluez.service
sudo pip3 install pybluez
echo "Clonning the git repo..."
git clone --branch python-script https://github.com/rizlas/open-auto-pro-sync
cd open-auto-pro-sync/oap_sync_script
sudo mkdir -p /usr/local/bin
sudo cp bt_rfcomm_server.py /usr/local/bin
sudo cp bt_rfcomm_server.service /etc/systemd/system
sudo systemctl enable bt_rfcomm_server.service
sudo systemctl start bt_rfcomm_server.service
echo "Waiting 5sec for service to boot up..."
sleep 5
if [ $(systemctl is-active --quiet bt_rfcomm_server.service) ]; then
    echo "Service is running, all good!"
else
    echo "BT services are not running! Check your setup."
fi
echo "Changing TemperatureSensorDescriptor to /opt/sensor"
sudo sed -i '/^TemperatureSensorDescriptor/ s#$#/opt/sensor#' /home/pi/.openauto/config/openauto_system.ini 
sudo timedatectl set-ntp false
echo "Try restarting the RPI for good measure"
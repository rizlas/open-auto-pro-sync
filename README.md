
# <div align="center"><img src="https://raw.githubusercontent.com/rizlas/open-auto-pro-sync/master/Icon_OAPS.png" width="100" height="100" /><br /> OpenAutoPro Sync</div>


If you are a happy owner of OpenAutoPro (https://bluewavestudio.io/index.php/bluewave-shop/openauto-pro-detail) you will have noticed that, unfortunately, raspberry clock is not synchronized due to the absence of a RTC module. The lack of this module, for example, will not allow using the OpenAutoPro automatic day and night switch. Of course there are other options to enable it, but all of them, include: installing cables, modules or sensors. So since 99% of the time my infotainment is connected to my phone, I thought I could retrieve what I needed from my phone and sync raspberry via bluetooth. To fulfill those needs i developed OpenAutoPro Sync that will add the following features to OpenAutoPro infotainment system:

1. Time sync
2. Geolocation sunrise and sunset time (for day and night feature)
3. Current outside temperature

All this without even a cable (obviously you will need a smartphone with internet and bluetooth).

### Time Sync

Current time is retrieved directly from your phone, no internet needed.

### Geolocation sunrise and sunset time

Sunrise and sunset times are provided by www.yr.no via API so any change must coop with their usage limits. If you want to update your times open the app and click "Get sunrise and sunset time".

### Temperature

Temperature is gathered from www.yr.no API service, again here, any software modification must comply with their Terms of Service. Script will emulate a DS18B20 sensor. At the moment the data retrieved is based only on latitude and longitude, future developments will also account the altitude or variations in latitude and longitude that require an update of the data.
## Automatic service setup
<pre><code>
Enter in the root of the repo:
<b>cd ~/open-auto-pro-sync</b>
Run the script:
<b>./setup-bt-services.sh</b>
</pre></code>
## Manual script installation steps

<pre><code>cd ~
sudo nano /etc/systemd/system/dbus-org.bluez.service
Add <b>"-c"</b> option at the end of ExecStart parameter
Add: <b>"ExecStartPost=/usr/bin/sdptool add SP"</b> 
after ExecStart
sudo pip3 install pybluez
git clone --branch python-script https://github.com/rizlas/open-auto-pro-sync
cd open-auto-pro-sync/oap_sync_script
sudo mkdir -p /usr/local/bin
sudo cp bt_rfcomm_server.py /usr/local/bin
sudo cp bt_rfcomm_server.service /etc/systemd/system
sudo systemctl enable bt_rfcomm_server.service
sudo systemctl start bt_rfcomm_server.service
nano /home/pi/.openauto/config/openauto_system.ini 
Change this setting <b>"TemperatureSensorDescriptor=/opt/sensor"</b>
Disable ntp sync (try with sudo timedatectl set-ntp false)
</pre></code>

## Troubleshooting

Check the status script via:
	
    systemctl status bt_rfcomm_server.service

The output should contain this line:

	 Active: active (running) since ...

Check script logs using:

	tail -f /tmp/bt_server.log

## Application

1. Download apk (https://github.com/rizlas/open-auto-pro-sync/raw/master/OpenAutoProSync.apk)
2. Install (enable unknown sources)
3. Grant permissions (if you want)

### Troubleshooting

Check if the service is running from 
	
	Developer options -> Running services and you should see OpenAutoPro Sync icon
	
if the service is not running reboot your phone. Further investigation to try to keep the service alive are in progress.

You can always sync OpenAuto Pro manually. Just open the app and click top right button. This will also restart the sync service.

##  Thanks to

- https://developer.yr.no/ for meteorological Apis
- https://github.com/pybluez/pybluez for rfcomm bluetooth server
- Icon made by smalllikeart from www.flaticon.com
- https://bluewavestudio.io/ for OpenAutoPro


Fell free to make pull requests, fork, destroy or whatever you like most. Any criticism is more than welcome.

<br/>
<p align="center">
<img src="https://raw.githubusercontent.com/rizlas/control-buttons-widget/master/Images/microsoft_net.png" width="70" height="70" />
<img src="https://raw.githubusercontent.com/rizlas/control-buttons-widget/master/Images/xamarin.png" width="200" height="84" /><br/>
<img src="https://raw.githubusercontent.com/rizlas/control-buttons-widget/master/Images/android.png" width="200" height="81" /><br/>
<img src="https://upload.wikimedia.org/wikipedia/commons/thumb/c/c3/Python-logo-notext.svg/768px-Python-logo-notext.svg.png" width="50" height="50" />
<img src="https://www.raspberrypi.org/app/uploads/2011/10/Raspi-PGB001.png" width="56" height="50" />
</p>

<br/>

<p align="center"><img src="https://avatars1.githubusercontent.com/u/8522635?s=96&v=4" /><br/>#followtheturtle</p>

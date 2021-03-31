using Android.Bluetooth;
using Android.Content;
using Java.Util;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Android.OS;
using Newtonsoft.Json;
using System.Globalization;
using Android.Support.V4.Content;
using Android.App;

namespace BT_OAP_Service
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { BluetoothDevice.ActionAclConnected })]
    class BtReceiver : BroadcastReceiver
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static long LastTriggerTime = 0;
        public override void OnReceive(Context context, Intent intent)
        {
            log.Info($"OnReceive BtReceiver Action {intent.Action}");
            Intent MessageIntent = new Intent(Constants.MessageReceiverFilter);

            // This if will avoid this https://stackoverflow.com/questions/8412714/broadcastreceiver-receives-multiple-identical-messages-for-one-event
            if (SystemClock.ElapsedRealtime() - LastTriggerTime > Constants.BtThresholdTrigger)
            {
                LastTriggerTime = SystemClock.ElapsedRealtime();

                DateTime SunTimeAge = DateTime.Parse(Utils.RetrievePreference(Constants.PrefSunTimeAge));

                if (System.Math.Abs((DateTime.Now - SunTimeAge).Days) > 6)
                {
                    string Latitude = Utils.RetrievePreference(Constants.PrefLatitude);
                    string Longitude = Utils.RetrievePreference(Constants.PrefLongitude);

                    if(Latitude != string.Empty && Longitude != string.Empty)
                    {
                        Utils.GetSunriseSunset(Latitude, Longitude, context);
                    }
                }

                // Maybe a cancellation token for this task could be a good idea
                Task.Run(async () =>
                {
                    BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
                    string Message = string.Empty;

                    if (adapter != null && adapter.IsEnabled)
                    {
                        BluetoothDevice device = (from bd in adapter.BondedDevices
                                                  where bd.Name == "OpenAuto-Pro"
                                                  select bd).FirstOrDefault();

                        if (device != null)
                        {
                            bool Connected = (bool)device.Class.GetMethod("isConnected", null).Invoke(device, null);

                            if (Connected)
                            {
                                var _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("94f39d29-7d6d-437d-973b-fba39e49d4ee"));

                                await _socket.ConnectAsync();

                                OapSync SyncValues = new OapSync();

                                if (intent.GetBooleanExtra("SyncAll", false) || intent.Action != null)
                                {
                                    SyncValues.Sunrise = Utils.RetrievePreference(Constants.PrefSunrise);
                                    SyncValues.Sunset = Utils.RetrievePreference(Constants.PrefSunset);
                                    SyncValues.Temperature = Utils.RetrievePreference(Constants.PrefTemperature);
                                }
                                else if (intent.GetBooleanExtra("SyncTemp", false))
                                {
                                    SyncValues.Temperature = Utils.RetrievePreference(Constants.PrefTemperature);
                                }
                                else if (intent.GetBooleanExtra("SyncSunTime", false))
                                {
                                    SyncValues.Sunrise = Utils.RetrievePreference(Constants.PrefSunrise);
                                    SyncValues.Sunset = Utils.RetrievePreference(Constants.PrefSunset);
                                }

                                SyncValues.Time = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

                                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SyncValues));
                                await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                                buffer = new byte[70];
                                await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length);

                                OapSyncResponse SyncResponse = JsonConvert.DeserializeObject<OapSyncResponse>(Encoding.UTF8.GetString(buffer));

                                string TimeNow = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");

                                if (SyncResponse.Time)
                                {
                                    Utils.StorePreference(Constants.PrefTimeSync, TimeNow);
                                }

                                if (SyncResponse.SunTime)
                                {
                                    Utils.StorePreference(Constants.PrefSunTimeSync, TimeNow);
                                }

                                if (SyncResponse.Temperature)
                                {
                                    Utils.StorePreference(Constants.PrefTempSync, TimeNow);
                                }

                                Message = "OpenAutoPro synced";
                                _socket.Close();
                            }
                            else
                            {
                                Message = "OpenAutoPro not connected";
                                log.Debug(Message);
                            }
                        }
                        else
                        {
                            Message = "Missing pairing with OpenAutoPro";
                            log.Debug(Message);
                        }
                    }
                    else
                    {
                        Message = "Bluetooth not found or not enabled";
                        log.Debug(Message);
                    }

                    SendMessage(context, intent, MessageIntent, Message);
                });
            }
            else
            {
                SendMessage(context, intent, MessageIntent, "Wait 10 seconds before syncing again");
            }
        }

        private void SendMessage(Context Context, Intent ReceivedIntent, Intent MessageIntent, string Message)
        {
            if (ReceivedIntent.GetBooleanExtra("SyncAll", false))
                MessageIntent.PutExtra("StopAnimation", true);

            MessageIntent.PutExtra("SnackMessage", Message);
            LocalBroadcastManager.GetInstance(Context).SendBroadcast(MessageIntent);
        }

        private string ParseDateTimeForOapConfigFile(string Date)
        {
            return Date != string.Empty ? DateTime.ParseExact(Date, "h:mm:ss tt", CultureInfo.InvariantCulture).ToString("H:mm") : string.Empty;
        }
    }
}
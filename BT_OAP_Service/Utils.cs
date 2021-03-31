using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Content;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BT_OAP_Service
{
    public static class Utils
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static bool StoreInProgress;

        public static void StorePreference(string Key, string Value)
        {
            var Preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var PreferencesEditor = Preferences.Edit();
            PreferencesEditor.PutString(Key, Value);
            bool Commited = PreferencesEditor.Commit();

            if(!Commited)
            {
                log.Error($"Preferences were not commited. Key: {Key}, Value: {Value}");
            }    
        }

        public static string RetrievePreference(string Key)
        {
            var Preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            return Preferences.GetString(Key, string.Empty);
        }

        public static void Sync(Context Context, string ExtraKey)
        {
            Intent BtIntent = new Intent(Context, typeof(BtReceiver));
            BtIntent.PutExtra(ExtraKey, true);

            Context.SendBroadcast(BtIntent);
        }

        public static void AlarmSetup(Context Context)
        {
            AlarmManager Manager = (AlarmManager)Context.GetSystemService(Context.AlarmService);
            Intent AlarmIntent = new Intent(Context, typeof(AlarmReceiver));
            PendingIntent AlarmPendingIntent = PendingIntent.GetBroadcast(Context, 0, AlarmIntent, 0);

            Manager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime(), AlarmManager.IntervalHalfHour, AlarmPendingIntent);
        }

        public static void GetSunriseSunset(string Latitude, string Longitude, Context Context)
        {
            if (!StoreInProgress)
            {
                StoreInProgress = true;

                Intent MessageIntent = new Intent(Constants.MessageReceiverFilter);
                var Offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

                RestClient Client = new RestClient("https://api.met.no/weatherapi/sunrise/2.0/.json");
                RestRequest Request = new RestRequest(Method.GET);
                Request.AddParameter("lat", Latitude);
                Request.AddParameter("lon", Longitude);
                Request.AddParameter("date", DateTime.Now.ToString("yyyy-MM-dd"));
                Request.AddParameter("offset", $"{(Offset < TimeSpan.Zero ? "-" : "+")}{Offset:hh\\:mm}");
                Request.AddHeader("User-Agent", Constants.YrForecastUserAgent);

                string FullUrl = Client.BuildUri(Request).ToString();

                string LastModifiedHeader = RetrievePreference(Constants.PrefLastModifiedHeaderSunTime);

                if (LastModifiedHeader != string.Empty)
                {
                    Request.AddHeader("If-Modified-Since", LastModifiedHeader);
                }

                Task.Run(async () =>
                {
                    IRestResponse Response = await Client.ExecuteAsync(Request);

                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        YrSunriseSunset SunriseSunset = JsonConvert.DeserializeObject<YrSunriseSunset>(Response.Content);
                    // No polar day handling, if someone will ever use this in polar region please drop me a pm to fix this
                    string Sunrise = SunriseSunset.Location.Time[0].Sunrise.Time.ToString("H:mm");
                        string Sunset = SunriseSunset.Location.Time[0].Sunset.Time.ToString("H:mm");

                        foreach (var H in Response.Headers)
                        {
                            if (H.Name == "Last-Modified")
                            {
                                StorePreference(Constants.PrefLastModifiedHeaderSunTime, H.Value.ToString());
                            }
                        }

                        StorePreference(Constants.PrefSunrise, Sunrise);
                        StorePreference(Constants.PrefSunset, Sunset);
                        StorePreference(Constants.PrefLatitude, Latitude);
                        StorePreference(Constants.PrefLongitude, Longitude);
                        StorePreference(Constants.PrefSunTimeAge, DateTime.Now.ToString());

                        Sync(Context, "SyncSunTime");

                        MessageIntent.PutExtra("SnackMessage", Resource.String.sbSuccessfulSunStore);
                        LocalBroadcastManager.GetInstance(Context).SendBroadcast(MessageIntent);
                    }
                    else if (Response.StatusCode == HttpStatusCode.NotModified)
                    {
                        log.Debug("Yr SunriseSunset Not Modified");
                        Sync(Context, "SyncSunTime");
                    }
                    else
                    {
                        if (Response.StatusCode != 0)
                        {
                            StringBuilder SbHeaders = new StringBuilder();

                            foreach (var H in Response.Headers)
                            {
                                SbHeaders.AppendLine(H.ToString());
                            }

                            log.Error($"StatusCode: {Response.StatusCode} Headers:{System.Environment.NewLine}{SbHeaders} FullUrl: {FullUrl} Response ErrorMessage: {Response.ErrorMessage}");
                        }
                        else
                        {
                            log.Error($"Response ErrorMessage: {Response.ErrorMessage} StatusCode: {Response.StatusCode} FullUrl: {FullUrl}");
                        }

                        MessageIntent.PutExtra("SnackMessage", Resource.String.sbSomethingWrong);
                        LocalBroadcastManager.GetInstance(Context).SendBroadcast(MessageIntent);
                    }

                    StoreInProgress = false;
                });
            }
        }
    }
}
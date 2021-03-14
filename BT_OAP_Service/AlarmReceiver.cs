using Android.Content;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BT_OAP_Service
{
    [BroadcastReceiver(Enabled = true)]
    public class AlarmReceiver : BroadcastReceiver
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public override void OnReceive(Context context, Intent intent)
        {
            log.Info("Interval AlarmReceiver");

            string ExpiresHeader = Utils.RetrievePreference(Constants.PrefExpiresHeader);
            bool GoAhead = true;

            if (ExpiresHeader != string.Empty)
            {
                if (DateTime.UtcNow < DateTime.Parse(ExpiresHeader))
                {
                    GoAhead = false;
                }
            }

            if (GoAhead)
            {
                string Latitude = Utils.RetrievePreference(Constants.PrefLatitude);
                string Longitude = Utils.RetrievePreference(Constants.PrefLongitude);

                if (Latitude != string.Empty && Longitude != string.Empty)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    RestClient Client = new RestClient("https://api.met.no/weatherapi/locationforecast/2.0/compact");
                    RestRequest Request = new RestRequest(Method.GET);
                    Request.AddParameter("lat", Latitude);
                    Request.AddParameter("lon", Longitude);
                    Request.AddHeader("User-Agent", Constants.YrForecastUserAgent);

                    string FullUrl = Client.BuildUri(Request).ToString();

                    string LastModifiedHeader = Utils.RetrievePreference(Constants.PrefLastModifiedHeader);

                    if (LastModifiedHeader != string.Empty)
                    {
                        Request.AddHeader("If-Modified-Since", LastModifiedHeader);
                    }

                    Task.Run(async () =>
                    {
                        IRestResponse Response = await Client.ExecuteAsync(Request);

                        if (Response.StatusCode == HttpStatusCode.OK)
                        {
                            YrForecast Forecast = JsonConvert.DeserializeObject<YrForecast>(Response.Content);
                            // Api responses timeseries seems to be always in order but better check
                            int TimeseriesIndex = 0;
                            DateTimeOffset DtSeriesToPick = Forecast.Properties.Timeseries[0].Time;

                            for (int i = 1; i < Forecast.Properties.Timeseries.Length; i++)
                            {
                                DateTimeOffset DtSeries = Forecast.Properties.Timeseries[i].Time;
                                if (DtSeries < DtSeriesToPick)
                                {
                                    DtSeriesToPick = DtSeries;
                                    TimeseriesIndex = i;
                                }
                            }

                            foreach (var H in Response.Headers)
                            {
                                if (H.Name == "Expires")
                                {
                                    Utils.StorePreference(Constants.PrefExpiresHeader, H.Value.ToString());
                                }
                                else if (H.Name == "Last-Modified")
                                {
                                    Utils.StorePreference(Constants.PrefLastModifiedHeader, H.Value.ToString());
                                }
                            }

                            Utils.StorePreference(Constants.PrefTemperature, Forecast.Properties.Timeseries[TimeseriesIndex].Data.Instant.Details.AirTemperature.ToString(CultureInfo.InvariantCulture));
                            Utils.StorePreference(Constants.PrefTempTimeRetrieved, DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm"));

                            Utils.Sync(context, "SyncTemp");
                        }
                        else if (Response.StatusCode == HttpStatusCode.NotModified)
                        {
                            log.Debug("Yr Not Modified");
                            Utils.Sync(context, "SyncTemp");
                        }
                        else if (Response.StatusCode != 0)  // Other response codes, code 0 normally occurs if network errors occur
                        {
                            StringBuilder SbHeaders = new StringBuilder();

                            foreach (var H in Response.Headers)
                            {
                                SbHeaders.AppendLine(H.ToString());
                            }

                            log.Error($"StatusCode: {Response.StatusCode} Headers:{Environment.NewLine}{SbHeaders} FullUrl: {FullUrl} Response ErrorMessage: {Response.ErrorMessage}");
                        }
                        else
                        {
                            log.Error($"Response ErrorMessage: {Response.ErrorMessage} FullUrl: {FullUrl}");
                        }
                    });
                }
                else
                {
                    log.Debug("No latitude and longitude coordinates stored");
                }
            }
            else
            {
                log.Debug("Yr Not Expired"); 
                Utils.Sync(context, "SyncTemp");
            }
        }
    }
}
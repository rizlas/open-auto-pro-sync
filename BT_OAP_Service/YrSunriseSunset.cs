using System;
using Newtonsoft.Json;

namespace BT_OAP_Service
{
    public partial class YrSunriseSunset
    {
        [JsonProperty("location")]
        public Location Location { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("time")]
        public Time[] Time { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }
    }

    public partial class Time
    {
        [JsonProperty("sunrise", NullValueHandling = NullValueHandling.Ignore)]
        public SunTime Sunrise { get; set; }

        [JsonProperty("sunset", NullValueHandling = NullValueHandling.Ignore)]
        public SunTime Sunset { get; set; }
    }

    public partial class SunTime
    {
        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }
    }
}

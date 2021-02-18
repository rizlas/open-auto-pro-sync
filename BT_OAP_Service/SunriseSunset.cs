using Newtonsoft.Json;

namespace BT_OAP_Service
{
    public class SunriseSunset
    {
        [JsonProperty("results")]
        public SunTime Results { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
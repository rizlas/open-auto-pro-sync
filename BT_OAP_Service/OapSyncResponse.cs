using Newtonsoft.Json;

namespace BT_OAP_Service
{
    public class OapSyncResponse
    {
        [JsonProperty("time")]
        public bool Time { get; set; }

        [JsonProperty("suntime")]
        public bool SunTime { get; set; }

        [JsonProperty("temperature")]
        public bool Temperature { get; set; }
    }
}
using System;
using Newtonsoft.Json;

namespace BT_OAP_Service
{
    public class YrForecast
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public class Geometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }
    }

    public class Properties
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("timeseries")]
        public Timeseries[] Timeseries { get; set; }
    }

    public class Meta
    {
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("units")]
        public Units Units { get; set; }
    }

    public class Units
    {
        [JsonProperty("air_pressure_at_sea_level")]
        public string AirPressureAtSeaLevel { get; set; }

        [JsonProperty("air_temperature")]
        public string AirTemperature { get; set; }

        [JsonProperty("cloud_area_fraction")]
        public string CloudAreaFraction { get; set; }

        [JsonProperty("precipitation_amount")]
        public string PrecipitationAmount { get; set; }

        [JsonProperty("relative_humidity")]
        public string RelativeHumidity { get; set; }

        [JsonProperty("wind_from_direction")]
        public string WindFromDirection { get; set; }

        [JsonProperty("wind_speed")]
        public string WindSpeed { get; set; }
    }

    public class Timeseries
    {
        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("instant")]
        public Instant Instant { get; set; }

        [JsonProperty("next_1_hours", NullValueHandling = NullValueHandling.Ignore)]
        public NextHours NextHours_1 { get; set; }

        [JsonProperty("next_6_hours", NullValueHandling = NullValueHandling.Ignore)]
        public NextHours NextHours_6 { get; set; }

        [JsonProperty("next_12_hours", NullValueHandling = NullValueHandling.Ignore)]
        public NextHours NextHours_12 { get; set; }
    }

    public class Instant
    {
        [JsonProperty("details")]
        public InstantDetails Details { get; set; }
    }

    public class InstantDetails
    {
        [JsonProperty("air_pressure_at_sea_level")]
        public double AirPressureAtSeaLevel { get; set; }

        [JsonProperty("air_temperature")]
        public double AirTemperature { get; set; }

        [JsonProperty("cloud_area_fraction")]
        public double CloudAreaFraction { get; set; }

        [JsonProperty("dew_point_temperature")]
        public double DewPointTemperature { get; set; }

        [JsonProperty("relative_humidity")]
        public double RelativeHumidity { get; set; }

        [JsonProperty("wind_from_direction")]
        public double WindFromDirection { get; set; }

        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }
    }

    public class NextHours
    {
        [JsonProperty("summary")]
        public Summary Summary { get; set; }

        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public NextHoursDetails Details { get; set; }
    }

    public class Summary
    {
        [JsonProperty("symbol_code")]
        public string SymbolCode { get; set; }
    }

    public class NextHoursDetails
    {
        [JsonProperty("precipitation_amount")]
        public double PrecipitationAmount { get; set; }
    }
}
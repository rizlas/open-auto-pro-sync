namespace BT_OAP_Service
{
    public static class Constants
    {
        public const int PermissionRequestLocation = 1;
        public const int PermissionRequestAll = 12;
        public const long BtThresholdTrigger = 10000;   // millisecond
        public const int TokenLocationTimeout = 90000;   // millisecond
        public const string PrefLatitude = "Latitude";
        public const string PrefLongitude = "Longitude";
        public const string PrefTemperature = "Temperature";
        public const string PrefSunrise = "Sunrise";
        public const string PrefSunset = "Sunset";
        public const string PrefTimeSync = "TimeSync";
        public const string PrefSunTimeSync = "SunTimeSync";
        public const string PrefTempSync = "TemperatureSync";
        public const string PrefFirstRunEver = "FirstRunEver";
        public const string PrefTempTimeRetrieved = "TemperatureTimeRetrieved";
        public const string PrefExpiresHeader = "ExpiresHeader";
        public const string PrefLastModifiedHeader = "LastModifiedHeader";
        public const string MessageReceiverFilter = "SnackMessage";
        public const string YrForecastUserAgent = "BT_OAP_ServiceApp/1.0 https://github.com/rizlas";
    }
}
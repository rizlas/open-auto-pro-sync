using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using NLog;

namespace BT_OAP_Service
{
    public static class Utils
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
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
    }
}
using Android.App;
using Android.Content;
using Android.OS;
using NLog;

namespace BT_OAP_Service
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted }, Categories =new[] { Intent.CategoryDefault })]
    class BootReceiver : BroadcastReceiver
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public override void OnReceive(Context context, Intent intent)
        {
            log.Debug($"OnReceive, triggered by: {intent.Action}");

            AlarmManager Manager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent AlarmIntent = new Intent(context, typeof(AlarmReceiver));
            PendingIntent AlarmPendingIntent = PendingIntent.GetBroadcast(context, 0, AlarmIntent, 0);

            Manager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime(), AlarmManager.IntervalHalfHour, AlarmPendingIntent);
        }
    }
}
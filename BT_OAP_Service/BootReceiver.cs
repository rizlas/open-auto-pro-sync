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

            Intent ServiceIntent = new Intent(context, typeof(OapService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(ServiceIntent);
            }
            else
            {
                context.StartService(ServiceIntent);
            }
        }
    }
}
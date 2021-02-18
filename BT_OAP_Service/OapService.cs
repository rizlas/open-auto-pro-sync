using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Content;
using Android.Bluetooth;
using NLog;

namespace BT_OAP_Service
{
    [Service(Exported = false)]
    public class OapService : Service
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            log.Debug("OnCreate Service");
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            log.Info("OnStartCommand");

            IntentFilter Filter = new IntentFilter();
            Filter.AddAction(BluetoothDevice.ActionAclConnected);
            BtReceiver Receiver = new BtReceiver();
            this.RegisterReceiver(Receiver, Filter);

            AlarmManager Manager = (AlarmManager)GetSystemService(Context.AlarmService);
            Intent AlarmIntent = new Intent(this, typeof(AlarmReceiver));
            PendingIntent AlarmPendingIntent = PendingIntent.GetBroadcast(this, 0, AlarmIntent, 0);

            Manager.SetRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime(), AlarmManager.IntervalHalfHour, AlarmPendingIntent);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using NLog.Config;
using NLog;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Android.Support.Design.Widget;
using System.Collections.Generic;
using Android.Content;
using Android.Widget;
using RestSharp;
using System.Net;
using System.Threading.Tasks;
using Android.Preferences;
using Android.Views.InputMethods;
using Android.Locations;
using Xamarin.Essentials;
using System;
using System.Globalization;
using System.Threading;
using Android.Views;
using Android.Support.V4.Content;
using Android.Views.Animations;
using Android.Support.V7.View.Menu;

namespace BT_OAP_Service
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISharedPreferencesOnSharedPreferenceChangeListener//, ILocationListener
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private CoordinatorLayout MainLayout;
        private Button BtnGetSunriseSunset;
        private Button BtnLocate;
        private TextInputEditText txtLatitude;
        private TextInputEditText txtLongitude;
        private TextView tvSunrise;
        private TextView tvSunset;
        private TextView tvTemperature;
        private TextView tvTimeSync;
        private TextView tvSunSync;
        private TextView tvTempSync;
        private string[] RequiredPermissions;
        private bool StoreInProgress;
        private AlertDialog ProgressAlertDialog;
        private CancellationTokenSource Source;
        private MessageReceiver MessagesReceiver;
        private ActionMenuItemView ActionSync;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            MainLayout = FindViewById<CoordinatorLayout>(Resource.Id.mainLayout);
            BtnGetSunriseSunset = FindViewById<Button>(Resource.Id.btnGetSunriseSunset);
            BtnLocate = FindViewById<Button>(Resource.Id.btnLocate);
            txtLatitude = FindViewById<TextInputEditText>(Resource.Id.latitude);
            txtLongitude = FindViewById<TextInputEditText>(Resource.Id.longitude);
            tvSunrise = FindViewById<TextView>(Resource.Id.tvSunrise);
            tvSunset = FindViewById<TextView>(Resource.Id.tvSunset);
            tvTemperature = FindViewById<TextView>(Resource.Id.tvTemperature);
            tvTimeSync = FindViewById<TextView>(Resource.Id.tvTimeSync);
            tvSunSync = FindViewById<TextView>(Resource.Id.tvSunSync);
            tvTempSync = FindViewById<TextView>(Resource.Id.tvTempSync);

            RequiredPermissions = new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.WriteExternalStorage };
            MessagesReceiver = new MessageReceiver(MainLayout);

            //This should be run only the first time the app is open ever
            if (Utils.RetrievePreference(Constants.PrefFirstRunEver) == string.Empty)
            {
                StartService();

                Utils.StorePreference(Constants.PrefFirstRunEver, "Done");
            }

            if (!HasPermissions())
            {
                ActivityCompat.RequestPermissions(this, RequiredPermissions, Constants.PermissionRequestAll);
            }

            BtnGetSunriseSunset.Click += BtnGetSunriseSunset_Click;
            BtnLocate.Click += BtnLocate_Click;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_action, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_sync)
            {
                ActionSync = this.FindViewById<ActionMenuItemView>(Resource.Id.action_sync);
                Animation rotation = AnimationUtils.LoadAnimation(this, Resource.Animation.rotate_refresh);
                rotation.RepeatCount = Animation.Infinite;
                ActionSync.StartAnimation(rotation);
                Snackbar.Make(MainLayout, Resource.String.sbSyncing, Snackbar.LengthLong).Show();

                Utils.Sync(this.ApplicationContext, "SyncAll");
                StartService();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnResume()
        {
            base.OnResume();

            XmlLoggingConfiguration xmlLoggingConfiguration = new XmlLoggingConfiguration(System.Xml.XmlTextReader.Create(Assets.Open("NLog.config")), null);
            LogManager.Configuration = xmlLoggingConfiguration;

            PreferenceManager.GetDefaultSharedPreferences(this).RegisterOnSharedPreferenceChangeListener(this);

            txtLatitude.Text = Utils.RetrievePreference(Constants.PrefLatitude);
            txtLongitude.Text = Utils.RetrievePreference(Constants.PrefLongitude);
            tvSunrise.Text = $"Sunrise: {Utils.RetrievePreference(Constants.PrefSunrise)}";
            tvSunset.Text = $"Sunset: {Utils.RetrievePreference(Constants.PrefSunset)}";

            if (Utils.RetrievePreference(Constants.PrefTemperature) != string.Empty)
            {
                tvTemperature.Text = $"Temperature: {Utils.RetrievePreference(Constants.PrefTemperature)}°C retrieved at{System.Environment.NewLine}{Utils.RetrievePreference(Constants.PrefTempTimeRetrieved)}";
            }

            tvTimeSync.Text = $"Time: {Utils.RetrievePreference(Constants.PrefTimeSync)}";
            tvSunSync.Text = $"Sunrise/Sunset: {Utils.RetrievePreference(Constants.PrefSunTimeSync)}";
            tvTempSync.Text = $"Temperature: {Utils.RetrievePreference(Constants.PrefTempSync)}";

            LocalBroadcastManager.GetInstance(this).RegisterReceiver(MessagesReceiver, new IntentFilter(Constants.MessageReceiverFilter));
        }

        protected override void OnPause()
        {
            base.OnPause();

            LocalBroadcastManager.GetInstance(this).UnregisterReceiver(MessagesReceiver);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            switch (key)
            {
                case Constants.PrefSunrise:
                    tvSunrise.Text = $"Sunrise: {sharedPreferences.GetString(key, string.Empty)}";
                    break;
                case Constants.PrefSunset:
                    tvSunset.Text = $"Sunset: {sharedPreferences.GetString(key, string.Empty)}";
                    break;
                case Constants.PrefLatitude:
                    txtLatitude.Text = sharedPreferences.GetString(key, string.Empty);
                    break;
                case Constants.PrefLongitude:
                    txtLongitude.Text = sharedPreferences.GetString(key, string.Empty);
                    break;
                case Constants.PrefTimeSync:
                    tvTimeSync.Text = $"Time: {sharedPreferences.GetString(key, string.Empty)}";
                    break;
                case Constants.PrefSunTimeSync:
                    tvSunSync.Text = $"Sunrise/Sunset: {sharedPreferences.GetString(key, string.Empty)}";
                    break;
                case Constants.PrefTempSync:
                    tvTempSync.Text = $"Temperature: {sharedPreferences.GetString(key, string.Empty)}";
                    break;
                case Constants.PrefTemperature:
                    tvTemperature.Text = $"Temperature: {sharedPreferences.GetString(key, string.Empty)}°C retrieved at{System.Environment.NewLine}{Utils.RetrievePreference(Constants.PrefTempTimeRetrieved)}";
                    break;
            }
        }

        private void BtnGetSunriseSunset_Click(object sender, System.EventArgs e)
        {
            InputMethodManager InputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            InputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);

            if (txtLatitude.Text != string.Empty && txtLongitude.Text != string.Empty && !StoreInProgress)
            {
                string Latitude = TruncateLatitudeLongitude(double.Parse(txtLatitude.Text, CultureInfo.InvariantCulture));
                string Longitude = TruncateLatitudeLongitude(double.Parse(txtLongitude.Text, CultureInfo.InvariantCulture));

                StoreInProgress = true;
                RestClient Client = new RestClient("https://api.sunrise-sunset.org/json?");
                RestRequest Request = new RestRequest(Method.GET);
                Request.AddParameter("lat", Latitude);
                Request.AddParameter("lng", Longitude);
                Snackbar.Make(MainLayout, Resource.String.sbGettingSunriseSunset, Snackbar.LengthLong).Show();

                Task.Run(async () =>
                {
                    IRestResponse<SunriseSunset> Response = await Client.ExecuteAsync<SunriseSunset>(Request);

                    if (Response.StatusCode == HttpStatusCode.OK && Response.Data.Status == "OK")
                    {
                        string Sunrise = DateTime.Parse(Response.Data.Results.Sunrise).ToLocalTime().ToString("H:mm");
                        string Sunset = DateTime.Parse(Response.Data.Results.Sunset).ToLocalTime().ToString("H:mm");

                        Utils.StorePreference(Constants.PrefSunrise, Sunrise);
                        Utils.StorePreference(Constants.PrefSunset, Sunset);
                        Utils.StorePreference(Constants.PrefLatitude, Latitude);
                        Utils.StorePreference(Constants.PrefLongitude, Longitude);

                        Utils.Sync(this.ApplicationContext, "SyncSunTime");

                        Snackbar.Make(MainLayout, Resource.String.sbSuccessfulSunStore, Snackbar.LengthLong).Show();
                    }
                    else
                    {
                        if (Response.StatusCode != 0)
                        {
                            log.Error($"StatusCode: {Response.StatusCode} Data: {Response.Data.Status} FullUrl: {Client.BuildUri(Request)} Response ErrorMessage: {Response.ErrorMessage}");
                        }
                        else
                        {
                            log.Error($"Response ErrorMessage: {Response.ErrorMessage} StatusCode: {Response.StatusCode}");
                        }

                        Snackbar.Make(MainLayout, Resource.String.sbSomethingWrong, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
                    }

                    StoreInProgress = false;
                });
            }
        }

        private void BtnLocate_Click(object sender, System.EventArgs e)
        {
            InputMethodManager InputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            InputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);

            AlertDialog.Builder ProgressAlertDialogBuilder = new AlertDialog.Builder(this);
            View frame = LayoutInflater.Inflate(Resource.Layout.pb_IndeterminateWithHorizontalText, MainLayout, false);
            ProgressAlertDialogBuilder.SetView(frame);
            ProgressAlertDialogBuilder.SetTitle("Locate");
            ProgressBar ProgressBar = frame.FindViewById<ProgressBar>(Resource.Id.AdPbMaterialProgressBar);
            ProgressBar.Indeterminate = true;
            TextView TextBoxProgressAlert = frame.FindViewById<TextView>(Resource.Id.AdPbTextBox);
            TextBoxProgressAlert.Text = "Localization in progress....";

            ProgressAlertDialog = ProgressAlertDialogBuilder.Create();
            ProgressAlertDialog.SetCancelable(false);
            ProgressAlertDialog.SetCanceledOnTouchOutside(false);
            ProgressAlertDialog.Show();

            Task.Run(async () =>
            {
                try
                {
                    Source = new CancellationTokenSource(Constants.TokenLocationTimeout);
                    Source.Token.Register(OnCancellationRequest);

                    GeolocationRequest Request = new GeolocationRequest(GeolocationAccuracy.Default);
                    var Location = await Geolocation.GetLocationAsync(Request, Source.Token);

                    if (Location != null)
                    {
                        Utils.StorePreference(Constants.PrefLatitude, TruncateLatitudeLongitude(Location.Latitude));
                        Utils.StorePreference(Constants.PrefLongitude, TruncateLatitudeLongitude(Location.Longitude));
                    }

                    // Side note for a feature, continuos location need to sync also sunrise and sunset
                    Snackbar.Make(MainLayout, Resource.String.sbLocateSuccessful, Snackbar.LengthLong).Show();
                }
                catch (FeatureNotSupportedException FnsEx)
                {
                    log.Error(FnsEx);
                    Snackbar.Make(MainLayout, Resource.String.sbNoGPSFound, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
                }
                catch (FeatureNotEnabledException FneEx)
                {
                    log.Error(FneEx);
                    Snackbar.Make(MainLayout, Resource.String.sbEnableGPS, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
                }
                catch (PermissionException PEx)
                {
                    log.Error(PEx);
                    if (this.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Denied)
                    {
                        if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
                        {
                            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, Constants.PermissionRequestLocation);
                        }
                        else
                        {
                            Snackbar.Make(MainLayout, Resource.String.sbEnableLocationPermission, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    Snackbar.Make(MainLayout, Resource.String.sbSomethingWrong, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
                }
                finally
                {
                    if (ProgressAlertDialog.IsShowing)
                    {
                        ProgressAlertDialog.Dismiss();
                    }

                    Source.Dispose();
                }
            });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            Dictionary<string, int> PermissionsDict = new Dictionary<string, int>();
            foreach (string permission in RequiredPermissions)
            {
                PermissionsDict.Add(permission, (int)Permission.Granted);
            }

            if (grantResults.Length > 0)
            {
                for (int i = 0; i < permissions.Length; i++)
                {
                    PermissionsDict[permissions[i]] = (int)grantResults[i];
                }

                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation) || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage))
                {
                    AlertDialog.Builder builderAlert = new AlertDialog.Builder(this);
                    builderAlert.SetTitle(Resource.String.alertDialogTitle);
                    builderAlert.SetMessage(Resource.String.alertDialogMessage);
                    builderAlert.SetPositiveButton(Resource.String.alertDialogYes, (senderYes, argYes) =>
                    {
                        ActivityCompat.RequestPermissions(this, RequiredPermissions, Constants.PermissionRequestAll);
                    });
                    builderAlert.SetNegativeButton(Resource.String.alertDialogNo, (senderNo, argNo) =>
                    {
                    });
                    builderAlert.SetCancelable(true);

                    AlertDialog dialogAlarm = builderAlert.Create();
                    dialogAlarm.SetCanceledOnTouchOutside(false);

                    dialogAlarm.Show();
                }
                else
                {
                    Snackbar.Make(MainLayout, Resource.String.sbPermission, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
                }
            }
        }

        private void OnCancellationRequest()
        {
            if (ProgressAlertDialog.IsShowing)
            {
                ProgressAlertDialog.Dismiss();
            }

            Snackbar.Make(MainLayout, Resource.String.sbTaskTimeout, Snackbar.LengthIndefinite).SetAction("OK", (View) => { }).Show();
        }

        private bool HasPermissions()
        {
            foreach (string permission in RequiredPermissions)
            {
                if (ActivityCompat.CheckSelfPermission(this, permission) != Permission.Granted)
                    return false;
            }

            return true;
        }

        private string TruncateLatitudeLongitude(double Value)
        {
            // Only 4 digit, no rounding
            return (Math.Truncate(Value * 10000) / 10000).ToString(CultureInfo.InvariantCulture);
        }

        private void StartService()
        {
            Intent ServiceIntent = new Intent(this.ApplicationContext, typeof(OapService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                this.ApplicationContext.StartForegroundService(ServiceIntent);
            }
            else
            {
                this.ApplicationContext.StartService(ServiceIntent);
            }
        }

        [BroadcastReceiver(Enabled = true, Exported = false)]
        class MessageReceiver : BroadcastReceiver
        {
            private readonly View _view;
            private readonly Android.Support.V7.Widget.Toolbar _toolbar;

            public MessageReceiver()
            {

            }

            public MessageReceiver(View View)
            {
                _view = View;
                _toolbar = _view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            }

            public override void OnReceive(Context context, Intent intent)
            {
                ActionMenuItemView Sync = _toolbar.FindViewById<ActionMenuItemView>(Resource.Id.action_sync);

                if (Sync != null && intent.GetBooleanExtra("StopAnimation", false))
                {
                    Sync.ClearAnimation();
                }

                string Message = intent.GetStringExtra("SnackMessage");

                if (Message != null)
                {
                    Snackbar.Make(_view, Message, Snackbar.LengthLong).Show();
                }
            }
        }
    }
}
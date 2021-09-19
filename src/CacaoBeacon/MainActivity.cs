using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using Android.Bluetooth;
using Android.Bluetooth.LE;

namespace CacaoBeacon
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            Android.Widget.Button btn = FindViewById<Android.Widget.Button>(Resource.Id.button1);
            btn.Click += Btn_Click;

        }

        Android.Widget.TextView textResult;

        /// <summary>
        /// Beacon を送信する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Btn_Click");

            BluetoothLeAdvertiser advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;
            AdvertiseSettings settings = new AdvertiseSettings.Builder()
                     .SetAdvertiseMode(AdvertiseMode.LowLatency)
                     .SetTxPowerLevel(AdvertiseTx.PowerLow)
                     .SetConnectable(false)
                     .Build();

            ParcelUuid pUuid = new ParcelUuid(Java.Util.UUID.FromString("0000fd6f-0000-1000-8000-00805f9b34fb"));

            // var body1 = new byte[] { 0x1a };
            // var body2 = new byte[] { 0x6f, 0xfd, };
            var body3 = new byte[]
            {
                0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,
                0x22,0x22,0x22,0x22,
            };

            AdvertiseData data = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(false)
                    .AddServiceUuid(pUuid)
                    .AddServiceData(pUuid, body3)
                    .Build();

            // コールバックを設定する
            var textResult = FindViewById<Android.Widget.TextView>(Resource.Id.textView1);
            

            var advertisingCallback = new _AdvertiseCallback();
            advertisingCallback.eventStartSuccess += (settingsInEffect) => {
                System.Diagnostics.Debug.WriteLine("eventStartSuccess");
                textResult.Text = settingsInEffect.ToString();
            };
            advertisingCallback.eventStartFailure += (errorCode) => {
                System.Diagnostics.Debug.WriteLine("eventStartFailure");
                textResult.Text = errorCode.ToString();
            };


            System.Diagnostics.Debug.WriteLine("StartAdvertising");
            advertiser.StartAdvertising(settings, data, advertisingCallback);
        }


        public class _AdvertiseCallback : AdvertiseCallback {

            public event Action<AdvertiseSettings> eventStartSuccess;
            public event Action<AdvertiseFailure> eventStartFailure;

            public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
            {
                base.OnStartSuccess(settingsInEffect);
                eventStartSuccess?.Invoke(settingsInEffect);
            }
            public override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode)
            {
                base.OnStartFailure(errorCode);
                eventStartFailure?.Invoke(errorCode);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

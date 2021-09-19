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
using OpenCacao.CacaoBeacon;
using System.Collections.Generic;

namespace CacaoBeaconSend
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

            this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;


            Android.Widget.Button btn1 = FindViewById<Android.Widget.Button>(Resource.Id.button1);
            btn1.Click += Btn1_Click;
            Android.Widget.Button btn2 = FindViewById<Android.Widget.Button>(Resource.Id.button2);
            btn2.Click += Btn2_Click;
            Android.Widget.Button btn3 = FindViewById<Android.Widget.Button>(Resource.Id.button3);
            btn3.Click += Btn3_Click;
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

        BluetoothLeAdvertiser _advertiser;

        /// <summary>
        /// Beacon を送信する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Btn_Click");

            _advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;
            AdvertiseSettings settings = new AdvertiseSettings.Builder()
                     .SetAdvertiseMode(AdvertiseMode.LowPower)
                     .SetTxPowerLevel(AdvertiseTx.PowerUltraLow)
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

            Android.Widget.TextView textResult = FindViewById<Android.Widget.TextView>(Resource.Id.textResult);
            Android.Widget.TextView textTEK = FindViewById<Android.Widget.TextView>(Resource.Id.textTEK);
            Android.Widget.TextView textRPI = FindViewById<Android.Widget.TextView>(Resource.Id.textRPI);

            textTEK.Text = "TEK: none";
            textRPI.Text = "RPI: none";

            // コールバックを設定する
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
            _advertiser.StartAdvertising(settings, data, advertisingCallback);
        }




        /// <summary>
        /// TEKに基づいてBeacon を送信する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Btn2_Click");

            _advertiser = BluetoothAdapter.DefaultAdapter.BluetoothLeAdvertiser;
            AdvertiseSettings settings = new AdvertiseSettings.Builder()
                     .SetAdvertiseMode(AdvertiseMode.LowPower)
                     .SetTxPowerLevel(AdvertiseTx.PowerUltraLow)
                     .SetConnectable(false)
                     .Build();

            ParcelUuid pUuid = new ParcelUuid(Java.Util.UUID.FromString("0000fd6f-0000-1000-8000-00805f9b34fb"));

            // TEK を取得する
            byte[] TEK = CBPack.makeTEK();
            // RPIs を計算する
            // 現在時刻の RPI を取得する
            byte[] RPI = CBPack.getRPI(TEK, DateTime.Now);

            var body = new List<byte>();
            body.AddRange(RPI);
            body.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, });
            var body3 = body.ToArray();


            AdvertiseData data = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(false)
                    .AddServiceUuid(pUuid)
                    .AddServiceData(pUuid, body3)
                    .Build();

            Android.Widget.TextView textResult = FindViewById<Android.Widget.TextView>(Resource.Id.textResult);
            Android.Widget.TextView textTEK = FindViewById<Android.Widget.TextView>(Resource.Id.textTEK);
            Android.Widget.TextView textRPI = FindViewById<Android.Widget.TextView>(Resource.Id.textRPI);

            textTEK.Text = "TEK: " + BitConverter.ToString(TEK).Replace("-", "").ToLower();
            textRPI.Text = "RPI: " + BitConverter.ToString(RPI).Replace("-", "").ToLower();

            // コールバックを設定する
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
            _advertiser.StartAdvertising(settings, data, advertisingCallback);
        }

        /// <summary>
        /// Beacon を停止する
        /// 
        /// どうも停止しないのでアプリを終了するしかない？
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn3_Click(object sender, EventArgs e)
        {
            if (_advertiser == null) return;

            var advertisingCallback = new _AdvertiseCallback();
            Android.Widget.TextView textResult = FindViewById<Android.Widget.TextView>(Resource.Id.textResult);
            advertisingCallback.eventStartSuccess += (settingsInEffect) => {
                System.Diagnostics.Debug.WriteLine("eventStartSuccess");
                textResult.Text = settingsInEffect.ToString();
            };
            advertisingCallback.eventStartFailure += (errorCode) => {
                System.Diagnostics.Debug.WriteLine("eventStartFailure");
                textResult.Text = errorCode.ToString();
            };
            _advertiser.StopAdvertising(advertisingCallback);
            _advertiser = null;
        }

        public class _AdvertiseCallback : AdvertiseCallback
        {

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
    }
}

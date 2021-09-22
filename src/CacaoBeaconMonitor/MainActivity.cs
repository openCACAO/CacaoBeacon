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
using System.Linq;
using System.Collections.Generic;
using OpenCacao.CacaoBeacon;
using Android.Content;

namespace CacaoBeaconMonitor
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

            Android.Widget.Button btn = FindViewById<Android.Widget.Button>(Resource.Id.button1);
            btn.Click += OnRecvClick;
            Android.Widget.Button btn2 = FindViewById<Android.Widget.Button>(Resource.Id.button2);
            btn2.Click += OnDownloadClick;

            Android.Widget.ListView listview = FindViewById<Android.Widget.ListView>(Resource.Id.listView1);
            _adapter = new BeaconAdapter(this);
            _adapter.Items = new List<RPI> {
                new RPI()
                {
                    Key = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, },
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(10),
                },

            };
            listview.Adapter = _adapter;

            Android.Widget.ListView listview2 = FindViewById<Android.Widget.ListView>(Resource.Id.listView2);
            _adapter_tek = new  TekAdapter(this);
            _adapter_tek.Items = new List<TEK>();
            listview2.Adapter = _adapter_tek;



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



        BluetoothLeScanner scanner;

        /// <summary>
        /// スキャン開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnRecvClick(object sender, EventArgs eventArgs)
        {
            scanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;
            var callback = new _ScanCallback();
            callback.eventScanResult += Callback_eventScanResult;
            scanner.StartScan(callback);

            var lv1 = FindViewById<Android.Widget.ListView>(Resource.Id.listView1);
            var lv2 = FindViewById<Android.Widget.ListView>(Resource.Id.listView2);
            lv1.Visibility = ViewStates.Visible;
            lv2.Visibility = ViewStates.Invisible;


        }

        List<string> maclist = new List<string>();
        
        public static CBReceiver cbreciever = new CBReceiver();
        BeaconAdapter _adapter;
        TekAdapter _adapter_tek;

        public class BeaconAdapter : Android.Widget.BaseAdapter<RPI>
        {
            Context _context;
            LayoutInflater _layoutInflater = null;

            public List<RPI> Items { get; set; } = new List<RPI>();
            public BeaconAdapter(Context context)
            {
                _context = context;
                _layoutInflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            }

            public override int Count => this.Items.Count;
            public override RPI this[int position] => this.Items[position];
            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View row = convertView;
                if (row == null)
                {
                    row = _layoutInflater.Inflate(Resource.Layout.lvitem, null, false);
                }
                var textKey = row.FindViewById<Android.Widget.TextView>(Resource.Id.key);
                var textStartTime = row.FindViewById<Android.Widget.TextView>(Resource.Id.starttime);
                var textEndTime = row.FindViewById<Android.Widget.TextView>(Resource.Id.endtime);

                var item = this.Items[position];
                textKey.Text = item.ToKeyString();
                textStartTime.Text = item.StartTime.ToString("HH:mm:ss");
                textEndTime.Text = item.EndTime.ToString("HH:mm:ss");
                return row;
            }
        }

        /// <summary>
        /// Beaconのスキャン
        /// </summary>
        /// <param name="callbackType"></param>
        /// <param name="result"></param>
        private void Callback_eventScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            var uuids = result.ScanRecord.ServiceUuids;
            var mac = result.Device.Address;
            var name = result.Device.Name;

            if (uuids == null || uuids.Count == 0) return;
            if (uuids.FirstOrDefault(t => t.ToString() == "0000fd6f-0000-1000-8000-00805f9b34fb") == null) return;

            // key 等を取得
            foreach (var it in result.ScanRecord.ServiceData)
            {
                var uuid = it.Key.ToString();
                var data = tohex(it.Value);
                cbreciever.Recv( it.Value[0..16] );
                _adapter.Items = cbreciever.RPIs;
                _adapter.NotifyDataSetChanged();
            }


            if (!maclist.Exists(t => t == mac))
            {
                maclist.Add(mac);

                System.Diagnostics.Debug.WriteLine($"----------------");
                System.Diagnostics.Debug.WriteLine($"received {DateTime.Now}");
                System.Diagnostics.Debug.WriteLine($"MAC: {mac}");
                System.Diagnostics.Debug.WriteLine($"NAME: {name}");
                System.Diagnostics.Debug.WriteLine($"ServiceUuids count:{uuids.Count}");
                foreach (var it in uuids)
                {
                    System.Diagnostics.Debug.WriteLine($"uuid: {it}");
                }

                var dataSections = result.ScanRecord.ServiceData;
                System.Diagnostics.Debug.WriteLine($"ServiceData count:{dataSections.Count}");
                foreach (var it in dataSections)
                {
                    var uuid = it.Key.ToString();
                    var data = tohex(it.Value);
                    System.Diagnostics.Debug.WriteLine($"  uuid:{uuid} data:{data}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"** MAC: {mac} at received {DateTime.Now}");
            }

            string tohex(byte[] data)
            {
                return BitConverter.ToString(data).Replace("-", "").ToLower();
            }
        }

        public class _ScanCallback : ScanCallback
        {
            public event Action<ScanCallbackType, ScanResult> eventScanResult;
            public event Action<ScanFailure> eventScanFailed;
            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);
                eventScanResult?.Invoke(callbackType, result);
            }

            public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
            {
                base.OnScanFailed(errorCode);
                eventScanFailed?.Invoke(errorCode);
            }
            public override void OnBatchScanResults(IList<ScanResult> results)
            {
                base.OnBatchScanResults(results);
            }
        }


        /// <summary>
        /// TEKのダウンロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private async void OnDownloadClick(object sender, EventArgs eventArgs)
        {
            var teks = await ExposureNotification.DownloadBatchAsync();
            _adapter_tek.Items = teks.Take(100).ToList();
            _adapter_tek.NotifyDataSetChanged();

            var lv1 = FindViewById<Android.Widget.ListView>(Resource.Id.listView1);
            var lv2 = FindViewById<Android.Widget.ListView>(Resource.Id.listView2);
            lv1.Visibility = ViewStates.Invisible;
            lv2.Visibility = ViewStates.Visible;

        }

        public class TekAdapter : Android.Widget.BaseAdapter<TEK>
        {
            Context _context;
            LayoutInflater _layoutInflater = null;

            public List<TEK> Items { get; set; } = new List<TEK>();
            public TekAdapter(Context context)
            {
                _context = context;
                _layoutInflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            }

            public override int Count => this.Items.Count;
            public override TEK this[int position] => this.Items[position];
            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View row = convertView;
                if (row == null)
                {
                    row = _layoutInflater.Inflate(Resource.Layout.lvitem, null, false);
                }
                var textKey = row.FindViewById<Android.Widget.TextView>(Resource.Id.key);
                var textStartTime = row.FindViewById<Android.Widget.TextView>(Resource.Id.starttime);
                var textEndTime = row.FindViewById<Android.Widget.TextView>(Resource.Id.endtime);

                var item = this.Items[position];
                textKey.Text = BitConverter.ToString(item.Key).Replace("-", "").ToLower();
                textStartTime.Text = item.Date.ToString();
                textEndTime.Text = "";
                return row;
            }
        }
    }

}

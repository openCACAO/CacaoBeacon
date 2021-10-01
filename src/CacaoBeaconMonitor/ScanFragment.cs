using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using OpenCacao.CacaoBeacon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CacaoBeaconMonitor
{
    public class ScanFragment : AndroidX.Fragment.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.content_scan, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var btn = this.View.FindViewById<Android.Widget.Button>(Resource.Id.buttonScan);
            btn.Click += OnRecvClick;


            var listview = View.FindViewById<Android.Widget.ListView>(Resource.Id.listViewScan);
            _adapter = new BeaconAdapter(this.Context);
            _adapter.Items = new List<RPI> {
                new RPI()
                {
                    Key = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, },
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(10),
                },

            };
            listview.Adapter = _adapter;
        }

        BluetoothLeScanner scanner;
        List<string> maclist = new List<string>();
        
        public static CBReceiver cbreciever = new CBReceiver()
        {
            Storage = new CBStorageSQLite(),
        };

        BeaconAdapter _adapter;
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
        /// スキャン開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnRecvClick(object sender, EventArgs eventArgs)
        {
            cbreciever.LoadStorage();


            scanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;
            var callback = new _ScanCallback();
            callback.eventScanResult += Callback_eventScanResult;
            scanner.StartScan(callback);

            // _adapter = new BeaconAdapter(this);
            // var lv1 = FindViewById<Android.Widget.ListView>(Resource.Id.listView1);
            // lv1.Adapter = _adapter;
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
                cbreciever.Recv(it.Value[0..16]);
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

    }
}
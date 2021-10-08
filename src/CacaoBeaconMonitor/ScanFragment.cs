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
using Environment = System.Environment;
using System.IO;
using System.Threading.Tasks;

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

            var btn2 = this.View.FindViewById<Android.Widget.Button>(Resource.Id.buttonScanSave);
            btn2.Click += OnSaveClick;

            var btn3 = this.View.FindViewById<Android.Widget.Button>(Resource.Id.buttonScanReset);
            btn3.Click += OnResetClick;

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

        BluetoothLeScanner _scanner = null;
        _ScanCallback _callback = null;
        List<string> maclist = new List<string>();

        public CBReceiver cbreciever = new CBReceiver();

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
                var textRssi = row.FindViewById<Android.Widget.TextView>(Resource.Id.rssi);

                var item = this.Items[position];
                textKey.Text = item.ToKeyString();
                textStartTime.Text = item.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                textEndTime.Text = item.EndTime.ToString("HH:mm:ss");
                textRssi.Text = item.RSSI_max.ToString();
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

            if (_scanner == null )
            {
                // スキャン開始
                _scanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;
                _callback = new _ScanCallback();
                _callback.eventScanResult += Callback_eventScanResult;
                _scanner.StartScan(_callback);
                var btn = this.View.FindViewById<Android.Widget.Button>(Resource.Id.buttonScan);
                btn.Text = "STOP";
            }
            else
            {
                // スキャン停止
                _scanner.StopScan(_callback);
                var btn = this.View.FindViewById<Android.Widget.Button>(Resource.Id.buttonScan);
                btn.Text = "START";
                _scanner = null;
            }
        }

        /// <summary>
        /// 画面更新タイミング
        /// </summary>
        private DateTime _updateTime = DateTime.Now;

        private class KeepRPI
        {
            public DateTime RecvTime { get; set; }
            public byte[] Key { get; set; }
        }
        private List<KeepRPI> _keeprpi = new List<KeepRPI>();

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
                cbreciever.Recv(it.Value[0..16], DateTime.Now, (short)result.Rssi, 0);


                // 直近5分間のRPIをキープする
                var rpi = it.Value[0..16];
                var item = _keeprpi.FirstOrDefault(t => t.Key.SequenceEqual(rpi));
                if ( item == null )
                {
                    _keeprpi.Add(new KeepRPI { RecvTime = DateTime.Now, Key = rpi });
                } 
                else
                {
                    item.RecvTime = DateTime.Now;
                }

                // 前回から5秒過ぎていれば画面を更新する
                if (_updateTime.AddSeconds(5) < DateTime.Now)
                {
                    _adapter.Items = cbreciever.RPIs.OrderByDescending(t => t.StartTime).ToList();
                    _adapter.NotifyDataSetChanged();
                    // 5秒ごとにキープリストを更新
                    // 同時接触数を直近30秒間に制限
                    _keeprpi = _keeprpi.Where(t => t.RecvTime > DateTime.Now.AddSeconds(-30)).ToList();
                    _updateTime = DateTime.Now;

                    var textKeepCount = this.View.FindViewById<Android.Widget.TextView>(Resource.Id.keepCount);
                    var textUpdateTime = this.View.FindViewById<Android.Widget.TextView>(Resource.Id.updateTime);
                    textKeepCount.Text = _keeprpi.Count.ToString();
                    textUpdateTime.Text = _updateTime.ToString("HH:mm:ss");

                }
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
        /// データを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnSaveClick(object sender, EventArgs eventArgs)
        {
            if (cbreciever == null) return;

            var contextRef = new WeakReference<Context>(this.Context);
            contextRef.TryGetTarget(out var c);
            var dir = c.GetExternalFilesDir(null).AbsolutePath;
            var outpath = System.IO.Path.Combine(dir, $"caraodb-{DateTime.Now.ToString("yyyyMMdd-HHmm")}.sqlite3");

            var dbpath = cbreciever.Storage.StoragePath;
            using (var fs = File.OpenRead(dbpath))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                File.WriteAllBytes(outpath, data);
            }

            var dlg = new AlertDialog.Builder(this.Context);
            dlg.SetMessage($"{outpath}に保存しました");
            dlg.SetPositiveButton( //OKボタンの処理
                "OK", (_,__) => { });
            dlg.Create().Show();
        }

        /// <summary>
        /// データベースを消去してリセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnResetClick(object sender, EventArgs eventArgs)
        {
            var dlg = new AlertDialog.Builder(this.Context);
            dlg.SetMessage($"収集データを消去しますか？");
            dlg.SetPositiveButton( //OKボタンの処理
                "OK", (_, __) => {

                    if (_scanner != null)
                    {
                        // いったん止めてからリセットする
                        _scanner.StopScan(_callback);
                    }
                    cbreciever.Storage.Reset();
                    cbreciever.RPIs.Clear();
                    // _adapter.Items = cbreciever.RPIs.OrderByDescending(t => t.StartTime).ToList();
                    // _adapter.NotifyDataSetChanged();

                    if (_scanner != null)
                    {
                        _scanner.StartScan(_callback);
                    }
                });
            dlg.SetNegativeButton( //Cancelボタンの処理
                "Cancel", (_, __) => { });
            dlg.Create().Show();

        }
    }
}
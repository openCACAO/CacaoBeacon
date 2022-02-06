using Android.App;
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
    public class DownloadFragment : AndroidX.Fragment.App.Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            return inflater.Inflate(Resource.Layout.content_download, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var btn = View.FindViewById<Button>(Resource.Id.buttonDownload);
            btn.Click += OnDownloadClick;

        }

        /// <summary>
        /// TEKのダウンロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private async void OnDownloadClick(object sender, EventArgs eventArgs)
        {

            var teks = await ExposureNotification.DownloadBatchAsync();
            
            var adapter = new TekAdapter(this.Context);
            adapter.Items = teks.Take(100).ToList();
            var lv1 = View.FindViewById<Android.Widget.ListView>(Resource.Id.listViewTEK);
            lv1.Adapter = adapter;
        }

        public class TekAdapter : Android.Widget.BaseAdapter<TemporaryExposureKey>
        {
            Context _context;
            LayoutInflater _layoutInflater = null;

            public List<TemporaryExposureKey> Items { get; set; } = new List<TemporaryExposureKey>();
            public TekAdapter(Context context)
            {
                _context = context;
                _layoutInflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            }

            public override int Count => this.Items.Count;
            public override TemporaryExposureKey this[int position] => this.Items[position];
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
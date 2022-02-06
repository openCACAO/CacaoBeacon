using OpenCacao.CacaoBeacon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;


namespace BeaconMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        ViewModel _vm;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = new ViewModel();
            this.DataContext = _vm;
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            _vm.StartScan();
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            _vm.StopScan();

        }
    }

    public class ViewModel : Prism.Mvvm.BindableBase
    {
        BluetoothLEAdvertisementWatcher watcher;
        CBReceiver cbreceiver = new CBReceiver();
        public ObservableCollection<_RPI> Items { get; set; }
        Dispatcher _dispatcher; 

        public ViewModel() :base()
        {
            watcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };
            watcher.Received += Watcher_Received;
            Items = new ObservableCollection<_RPI>();
            _dispatcher = Dispatcher.CurrentDispatcher;

        }

        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var uuids = args.Advertisement.ServiceUuids;
            var mac = string.Join(":",
                        BitConverter.GetBytes(args.BluetoothAddress).Reverse()
                        .Select(b => b.ToString("X2"))).Substring(6);
            var name = args.Advertisement.LocalName;

            if (uuids.Count == 0) return;
            if (uuids.FirstOrDefault(t => t.ToString() == "0000fd6f-0000-1000-8000-00805f9b34fb") == Guid.Empty) return;

            foreach (var it in args.Advertisement.DataSections)
            {
                if (it.DataType == 0x16 && it.Data.Length >= 2 + 16)
                {
                    byte[] data = new byte[it.Data.Length];
                    DataReader.FromBuffer(it.Data).ReadBytes(data);
                    if (data[0] == 0x6f && data[1] == 0xfd)
                    {
                        var rpi = data[2..18];
                        cbreceiver.Recv(rpi, DateTime.Now, args.RawSignalStrengthInDBm, args.BluetoothAddress);


                        var item = cbreceiver.RPIs.FirstOrDefault(t => t.Key.SequenceEqual(rpi) == true);
                        if ( item != null )
                        {
                            var item2 = this.Items.FirstOrDefault(t => t.Key == item.ToKeyString());
                            if (item2 == null )
                            {
                                // 新規に追加
                                _dispatcher.Invoke(() =>
                                {
                                    this.Items.Add(_RPI.FromRPI(item));
                                });
                            } else
                            {
                                _dispatcher.Invoke(() => {
                                    // 更新
                                    item2.EndTime = item.EndTime.DateTime;
                                    item2.RSSI_max = item.RssiMax;
                                    item2.RSSI_min = item.RssiMin;
                                });
                            }
                        }
                    }
                }
            }
        }

        public class _RPI : Prism.Mvvm.BindableBase
        {
            private string _Key;
            private DateTime _StartTime;
            private DateTime _EndTime;
            private int _RSSI_min;
            private int _RSSI_max;
            private string _MAC;

            public string Key { get => _Key; set => SetProperty(ref _Key, value, nameof(Key)); }
            public DateTime StartTime { get => _StartTime; set => SetProperty(ref _StartTime, value, nameof(StartTime)); }
            public DateTime EndTime { get => _EndTime; set => SetProperty(ref _EndTime, value, nameof(EndTime)); }
            public int RSSI_min { get => _RSSI_min; set => SetProperty(ref _RSSI_min, value, nameof(RSSI_min)); }
            public int RSSI_max { get => _RSSI_max; set => SetProperty(ref _RSSI_max, value, nameof(RSSI_max)); }
            public string MAC { get => _MAC; set => SetProperty(ref _MAC, value, nameof(MAC)); }

            public static _RPI FromRPI(RotatingProximityIdentifier rpi)
            {
                return new _RPI()
                {
                    Key = rpi.ToKeyString(),
                    StartTime = rpi.StartTime.DateTime,
                    EndTime = rpi.EndTime.DateTime,
                    RSSI_min = rpi.RssiMin,
                    RSSI_max = rpi.RssiMax,
                    MAC = rpi.toMacString()
                };
            }
        }



        public void StartScan()
        {
            watcher.Start();
        }
        public void StopScan()
        {
            watcher.Stop();
        }

    }

    
}

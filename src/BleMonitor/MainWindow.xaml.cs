using System;
using System.Collections.Generic;
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
using Windows.Devices.Bluetooth.Advertisement;
using System.Diagnostics;

namespace BleMonitor
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

        BluetoothLEAdvertisementWatcher watcher;


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += Watcher_Received;
        }


        /// <summary>
        /// 受信開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clickStart(object sender, RoutedEventArgs e)
        {
            watcher.Start();

        }

        /// <summary>
        /// 受信停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clickStop(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
        }

        /// <summary>
        /// 受信処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var uuids = args.Advertisement.ServiceUuids;
            var name = args.Advertisement.LocalName;

            var dt = DateTime.Now;
            Debug.WriteLine($"catch: {dt} count: {uuids.Count}");
            foreach ( var uuid in uuids )
            {
                if (uuid.ToString() == "0000fd6f-0000-1000-8000-00805f9b34fb") name = "Exposure Notifications";
                Debug.WriteLine($"  {name} {uuid} ");
            }

        }


    }
}

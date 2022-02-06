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
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Devices.Bluetooth;

namespace BeaconSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        BluetoothLEAdvertisementPublisher publisher;

        private void OnStartClick(object sender, RoutedEventArgs e)
        {

            // Create and initialize a new publisher instance.
            publisher = new BluetoothLEAdvertisementPublisher();
            var manufacturerData = new BluetoothLEManufacturerData();


            // Then, set the company ID for the manufacturer data. Here we picked an unused value: 0xFFFE
            manufacturerData.CompanyId = 0x004C;
            // Create the payload
            var writer = new DataWriter();
            byte[] dataArray = new byte[] {
                // last 2 bytes of Apple's iBeacon
                0x02, 0x15,
                // UUID e2 c5 6d b5 df fb 48 d2 b0 60 d0 f5 a7 10 96 e0
                0xe2, 0xc5, 0x6d, 0xb5,
                0xdf, 0xfb, 0x48, 0xd2,
                0xb0, 0x60, 0xd0, 0xf5,
                0xa7, 0x10, 0x96, 0xe0,
                // Major
                0x00, 0x00,
                // Minor
                0x00, 0x01,
                // TX power
                0xc5
            };
            manufacturerData.Data = writer.DetachBuffer();
            publisher.Advertisement.ManufacturerData.Add(manufacturerData);

            publisher.StatusChanged += Publisher_StatusChanged;
            publisher.Start();

        }

        private void Publisher_StatusChanged(BluetoothLEAdvertisementPublisher sender, BluetoothLEAdvertisementPublisherStatusChangedEventArgs args)
        {
            BluetoothLEAdvertisementPublisherStatus status = args.Status;
            BluetoothError error = args.Error;
            Debug.WriteLine($"Published Status: {status}, Error: {error}");
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            publisher.Stop();
        }
    }
}

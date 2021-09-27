using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

using OpenCacao.CacaoBeacon;

namespace console
{
    class Program
    {
        // BLEのスキャナ
        static BluetoothLEAdvertisementWatcher watcher;
        // MACアドレスの保持（ランダムなので意味はない）
        static List<ulong> maclist = new List<ulong>();

        // static BluetoothLEAdvertisementPublisher publisher;

        static void Main(string[] args)
        {
            Console.WriteLine("CacaoBeacon Reciever");
            // ストレージからロード
            cbreceiver.LoadStorage();


            // スキャンモードを設定
            watcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Passive
            };
            // スキャンしたときのコールバックを設定
            watcher.Received += Watcher_Received;
            // スキャン開始
            watcher.Start();
            // キーが押されるまで待つ
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }

        private static CBReceiver cbreceiver = new CBReceiver()
        {
            Storage = CBStorageSQLite.Create(),     // ストレージがあれば読み込む
        };


        private static void Watcher_Received(
            BluetoothLEAdvertisementWatcher sender,
            BluetoothLEAdvertisementReceivedEventArgs args)
        {

            var uuids = args.Advertisement.ServiceUuids;
            var mac = string.Join(":",
                        BitConverter.GetBytes(args.BluetoothAddress).Reverse()
                        .Select(b => b.ToString("X2"))).Substring(6);
            var name = args.Advertisement.LocalName;

            if (uuids.Count == 0) return;
            if (uuids.FirstOrDefault(t => t.ToString() == "0000fd6f-0000-1000-8000-00805f9b34fb") == Guid.Empty) return;

            // RPI を取得
            byte[] rpi = null;


            foreach (var it in args.Advertisement.DataSections)
            {
                if ( it.DataType == 0x16 && it.Data.Length >= 2 + 16)
                {
                    byte[] data = new byte[it.Data.Length];
                    DataReader.FromBuffer(it.Data).ReadBytes(data);
                    if ( data[0] == 0x6f && data[1] == 0xfd)
                    {
                        rpi = data[2..18];
                        cbreceiver.Recv(rpi, DateTime.Now, args.RawSignalStrengthInDBm, args.BluetoothAddress);
                    }
                }
            }

            Console.WriteLine("---");
            foreach (var it in cbreceiver.RPIs)
            {
                Console.WriteLine($"[{tohex(it.Key)}] {it.StartTime} {it.EndTime} {it.RSSI_min} {it.RSSI_max} dBm {it.toMacString()}");
            }
            Console.WriteLine("---");


            // MACアドレスはランダムなので、受信時は別の判定が必要
            // 実際には、RPI が切り替わるときに MAC アドレスも切り替わるので、これで十分

            if (!maclist.Exists(t => t == args.BluetoothAddress))
            {
                maclist.Add(args.BluetoothAddress);
                Console.WriteLine($"----------------");
                Console.WriteLine($"received {DateTime.Now}");
                Console.WriteLine($"MAC: {mac}");
                Console.WriteLine($"NAME: {name}");
                Console.WriteLine($"ServiceUuids count:{uuids.Count}");
                foreach (var it in uuids)
                {
                    Console.WriteLine($"uuid: {it}");
                }

                var dataSections = args.Advertisement.DataSections;
                var manufactures = args.Advertisement.ManufacturerData;
                Console.WriteLine($"dataSections count:{dataSections.Count}");
                foreach (var it in dataSections)
                {
                    Console.WriteLine($" type: {it.DataType.ToString("X2")} size: {it.Data.Length} data: {toHEX(it.Data)}");
                    byte[] data = new byte[it.Data.Length];
                    DataReader.FromBuffer(it.Data).ReadBytes(data);
                }
                Console.WriteLine($"manufactures count:{manufactures.Count}");
                foreach (var it in manufactures)
                {
                    Console.WriteLine($" size: {it.Data.Length} data: {toHEX(it.Data)}");
                }
            }
            else
            {
                Console.WriteLine($"** MAC: {mac} at received {DateTime.Now} rssi:{args.RawSignalStrengthInDBm}");
            }


            static string toHEX(IBuffer buf)
            {
                byte[] data = new byte[buf.Length];
                DataReader.FromBuffer(buf).ReadBytes(data);
                return BitConverter.ToString(data);
            }
            static string tohex( byte[] data )
            {
                return BitConverter.ToString(data).Replace("-", "").ToLower();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using OpenCacao.CacaoBeacon;
using System.Security.Cryptography;

namespace CacaoBeacon.Test
{
    public class TestCBStorageSQLite
    {
        /// <summary>
        /// 初期化
        /// </summary>
        [Fact]
        public void TestInitRPIs()
        {
            var storage = CBStorageSQLite.Create(true);
            var items = storage.RPI;
            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Fact]
        public void TestSaveRPIs()
        {
            var stroage = CBStorageSQLite.Create(true);
            var rpi = new RPI()
            {
                Key = new byte[] { 1, 2, 3, 4 },
                StartTime = new DateTime(2021, 1, 1, 0, 0, 0),
                EndTime = new DateTime(2021, 1, 1, 0, 1, 0),
                MAC = 1,
                RSSI_max = -10,
                RSSI_min = -20,
            };
            stroage.Add(rpi);

            var items = stroage.RPI;
            Assert.NotNull(items);
            Assert.Single(items);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, items[0].Key);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0), items[0].StartTime);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 1, 0), items[0].EndTime);
        }

        [Fact]
        public void TestSaveRPIs2()
        {
            var stroage = CBStorageSQLite.Create(true);
            var rpis = new List<RPI> {
                new RPI()
                {
                    Key = new byte[] {1,1,1 },
                },
                new RPI()
                {
                    Key = new byte[] {2,2,2 },
                },
            };
            stroage.AddRange(rpis);


            var items = stroage.RPI;
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            
            var rpis2 = new List<RPI> {
                new RPI() { Key = new byte[] {3,3,3 }, },
                new RPI() { Key = new byte[] {4,4,4 }, },
                new RPI() { Key = new byte[] {5,5,5, }, },
            };

            stroage.AddRange(rpis2);
            items = stroage.RPI;

            Assert.Equal(5, items.Count);
            Assert.Equal(1, items[0].Key[0]);
            Assert.Equal(5, items[4].Key[0]);
        }

        /// <summary>
        /// CBReceiver との組み合わせ
        /// </summary>


        [Fact]
        public void Test1()
        {
            var cbreceiver = new CBReceiver()
            {
                Storage = CBStorageSQLite.Create(true)
            };
            Assert.Empty(cbreceiver.RPIs);

            // 1つだけ受信する
            byte[] rpi = new byte[16];
            rpi[0] = 0x11;
            cbreceiver.Recv(rpi, DateTime.Now, -10);
            Assert.Single(cbreceiver.RPIs);

            var items = cbreceiver.Storage.RPI;
            Assert.Single(items);
        }

        // 異なる RPI を受信する
        [Fact]
        public void Test2()
        {
            var cbreceiver = new CBReceiver()
            {
                Storage = CBStorageSQLite.Create(true)
            };

            byte[] rpi1 = new byte[16];
            byte[] rpi2 = new byte[16];
            rpi1[0] = 0x11;
            rpi2[0] = 0x22;

            cbreceiver.Recv(rpi1, DateTime.Now, -10);
            cbreceiver.Recv(rpi2, DateTime.Now, -10);
            Assert.Equal(2, cbreceiver.RPIs.Count);

            var items = cbreceiver.Storage.RPI;
            Assert.Equal(2,items.Count);

        }

        // 同じ RPI を受信する
        // レコード数は1つで、日付が更新される
        [Fact]
        public void Test3()
        {
            var cbreceiver = new CBReceiver()
            {
                Storage = CBStorageSQLite.Create(true)
            };

            byte[] rpi1 = new byte[16];
            byte[] rpi2 = new byte[16];
            rpi1[0] = 0x11;
            rpi2[0] = 0x11;
            DateTime dt1 = new DateTime(2021, 2, 1, 12, 0, 0);
            DateTime dt2 = new DateTime(2021, 2, 1, 12, 0, 2);

            cbreceiver.Recv(rpi1, dt1, -10);
            cbreceiver.Recv(rpi2, dt2, -10);
            Assert.Equal(1, cbreceiver.RPIs.Count);
            Assert.Equal(dt1, cbreceiver.RPIs[0].StartTime);
            Assert.Equal(dt2, cbreceiver.RPIs[0].EndTime);

            var items = cbreceiver.Storage.RPI;
            Assert.Equal(1, items.Count);
            Assert.Equal(dt1, items[0].StartTime);
            Assert.Equal(dt2, items[0].EndTime);


        }

        // 同じ RPI は重ねる
        // 異なる RPI は追加でレコードを作る
        [Fact]
        public void Test4()
        {
            var cbreceiver = new CBReceiver()
            {
                Storage = CBStorageSQLite.Create(true)
            };

            byte[] rpi1 = new byte[16];
            byte[] rpi2 = new byte[16];
            byte[] rpi3 = new byte[16];
            rpi1[0] = 0x11;
            rpi2[0] = 0x22;
            rpi3[0] = 0x11;
            DateTime dt1 = new DateTime(2021, 2, 1, 12, 0, 0);
            DateTime dt2 = new DateTime(2021, 2, 1, 12, 0, 2);
            DateTime dt3 = new DateTime(2021, 2, 1, 12, 0, 2);


            cbreceiver.Recv(rpi1, dt1, -10);
            cbreceiver.Recv(rpi2, dt2, -10);
            cbreceiver.Recv(rpi3, dt3, -10);
            Assert.Equal(2, cbreceiver.RPIs.Count);


            var items = cbreceiver.Storage.RPI;
            Assert.Equal(2, items.Count);

        }


        // ストレージから復元する
        [Fact]
        public void Test5()
        {
            var cbreceiver = new CBReceiver()
            {
                Storage = CBStorageSQLite.Create(true)
            };

            byte[] rpi1 = new byte[16];
            byte[] rpi2 = new byte[16];
            rpi1[0] = 0x11;
            rpi2[0] = 0x22;

            cbreceiver.Recv(rpi1, DateTime.Now, -10);
            cbreceiver.Recv(rpi2, DateTime.Now, -10);
            Assert.Equal(2, cbreceiver.RPIs.Count);

            var items = cbreceiver.Storage.RPI;
            Assert.Equal(2, items.Count);

            // 復元
            cbreceiver = new CBReceiver()
            {
                Storage = CBStorageSQLite.Create()
            };
            cbreceiver.LoadStorage();
            Assert.Equal(2, cbreceiver.RPIs.Count);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using OpenCacao.CacaoBeacon;
using System.Security.Cryptography;

namespace CacaoBeacon.Test
{
    public class TestCBReceiver
    {
        [Fact]
        public void Test1()
        {
            var cbreceiver = new CBReceiver();
            Assert.Empty(cbreceiver.RPIs);

            // 1つだけ受信する
            byte[] rpi = new byte[16];
            rpi[0] = 0x11;
            cbreceiver.Recv(rpi, DateTime.Now, -10);
            Assert.Single(cbreceiver.RPIs);
        }

        // 異なる RPI を受信する
        [Fact]
        public void Test2()
        {
            var cbreceiver = new CBReceiver();

            byte[] rpi1 = new byte[16];
            byte[] rpi2 = new byte[16];
            rpi1[0] = 0x11;
            rpi2[0] = 0x22;
 
            cbreceiver.Recv(rpi1, DateTime.Now, -10);
            cbreceiver.Recv(rpi2, DateTime.Now, -10);
            Assert.Equal(2, cbreceiver.RPIs.Count);
        }

        // 同じ RPI を受信する
        // レコード数は1つで、日付が更新される
        [Fact]
        public void Test3()
        {
            var cbreceiver = new CBReceiver();

            byte[] rpi1 = new byte[16];
            byte[] rpi2 = new byte[16];
            rpi1[0] = 0x11;
            rpi2[0] = 0x11;
            DateTime dt1 = new DateTime(2021, 2, 1, 12,0, 0);
            DateTime dt2 = new DateTime(2021, 2, 1, 12, 0, 2);


            cbreceiver.Recv(rpi1, dt1, -10);
            cbreceiver.Recv(rpi2, dt2, -10);
            Assert.Equal(1, cbreceiver.RPIs.Count);
            Assert.Equal(dt1, cbreceiver.RPIs[0].StartTime);
            Assert.Equal(dt2, cbreceiver.RPIs[0].EndTime);
        }

        // 同じ RPI は重ねる
        // 異なる RPI は追加でレコードを作る
        [Fact]
        public void Test4()
        {
            var cbreceiver = new CBReceiver();

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
        }
    }
}

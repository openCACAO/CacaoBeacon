using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using OpenCacao.CacaoBeacon;
using System.Security.Cryptography;

namespace CacaoBeacon.Test
{
    public class TestCBPack
    {
        [Fact]
        public void TestMakeTEK()
        {
            // ランダムな16バイトの TEK を作成
            byte[] TEK = CBPack.makeTEK();
            Assert.NotNull(TEK);
            Assert.Equal(16, TEK.Length);
        }

        [Fact]
        public void TestMakeRPIs()
        {
            // TEK から RPIs を作成
            byte[] TEK = CBPack.makeTEK();
            List<byte[]> RPIs = CBPack.makeRPIs(TEK, 0);
            Assert.NotNull(RPIs);
            Assert.Equal(144, RPIs.Count);
            Assert.Equal(16, RPIs[0].Length);
        }

        [Fact]
        public void TestMakeRPIs2()
        {
            // TEK から RPIs を作成
            byte[] TEK = CBPack.makeTEK();

            // 同じ TEK であれば、同じ RPIs が生成される
            List<byte[]> RPIs = CBPack.makeRPIs(TEK, 10);
            Assert.NotNull(RPIs);
            Assert.Equal(144, RPIs.Count);
            Assert.Equal(16, RPIs[0].Length);

            // 同じ TEK であれば、同じ RPIs が生成される
            List<byte[]> RPIs2 = CBPack.makeRPIs(TEK, 10);
            Assert.NotNull(RPIs2);
            Assert.Equal(144, RPIs2.Count);

            for ( int i=0; i<144; i++ )
            {
                Assert.True(RPIs[i].SequenceEqual(RPIs2[i]));
            }
        }


        /// <summary>
        /// 現在時刻のRPIを取得するテスト
        /// </summary>
        [Fact]
        public void TestGetRPI()
        {
            byte[] tek = CBPack.makeTEK();

            // これは日本のローカルタイム +09:00
            DateTime now = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
            // Unix秒に直す
            DateTime utc = now.ToUniversalTime();
            // UTCの正午で rolling_start_interval_number = 0 となる
            utc = new DateTime(utc.Year, utc.Month, utc.Day, 0,0,0, DateTimeKind.Utc);
            var unix = new DateTimeOffset(utc.ToUniversalTime()).ToUnixTimeSeconds();
            long rolling_start_interval_number = unix / 600;

            byte[] rpi = CBPack.getRPI(tek, now);
            List<byte[]> rpis = CBPack.makeRPIs(tek, (ulong)rolling_start_interval_number );
            // RPI の場所を比較する
            string rpi_s = BitConverter.ToString(rpi);
            string rpis_s = BitConverter.ToString(rpis[(24-9)*6]);
            // 日本場合、+09:00 の位置にある
            Assert.Equal(rpi_s, rpis_s);
        }
        /// <summary>
        /// 現在時刻のRPIを取得するテスト
        /// </summary>
        [Fact]
        public void TestGetRPIbyUtc()
        {
            byte[] tek = CBPack.makeTEK();
            // UTCで渡す場合
            DateTime now = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // Unix秒に直す
            DateTime utc = now.ToUniversalTime();
            // UTCの正午で rolling_start_interval_number = 0 となる
            utc = new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
            var unix = new DateTimeOffset(utc.ToUniversalTime()).ToUnixTimeSeconds();
            long rolling_start_interval_number = unix / 600;

            byte[] rpi = CBPack.getRPI(tek, now);
            List<byte[]> rpis = CBPack.makeRPIs(tek, (ulong)rolling_start_interval_number);
            // RPI の場所を比較する
            string rpi_s = BitConverter.ToString(rpi);
            string rpis_s = BitConverter.ToString(rpis[0]);
            // UTCの場合は、先頭にある
            Assert.Equal(rpi_s, rpis_s);
        }
    }
}

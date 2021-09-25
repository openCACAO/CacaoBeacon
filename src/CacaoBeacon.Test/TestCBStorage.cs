using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using OpenCacao.CacaoBeacon;
using System.Security.Cryptography;

namespace CacaoBeacon.Test
{
    public class TestCBStorage
    {
        /// <summary>
        /// 初期化
        /// </summary>
        [Fact]
        public void TestInitRPIs()
        {
            CBStorage.InitRPIs();
            var items = CBStorage.LoadRPIs();

            Assert.NotNull(items);
            Assert.Equal(0, items.Count);
        }

        [Fact]
        public void TestSaveRPIs()
        {
            CBStorage.InitRPIs();
            var rpis = new List<RPI> {
                new RPI()
                {
                    Key = new byte[] { 1, 2, 3, 4 },
                    StartTime = new DateTime(2021, 1, 1, 0, 0, 0),
                    EndTime = new DateTime(2021, 1, 1, 0, 1, 0),
                    MAC = 1,
                    RSSI_max = -10,
                    RSSI_min = -20,
                }};
            CBStorage.AppendRPIs(rpis);
            var items = CBStorage.LoadRPIs();
            Assert.NotNull(items);
            Assert.Equal(1, items.Count);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, items[0].Key);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0), items[0].StartTime);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 1, 0), items[0].EndTime);
        }
        [Fact]
        public void TestSaveRPIs2()
        {
            CBStorage.InitRPIs();
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
            CBStorage.AppendRPIs(rpis);
            var items = CBStorage.LoadRPIs();
            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            var rpis2 = new List<RPI> {
                new RPI() { Key = new byte[] {3,3,3 }, },
                new RPI() { Key = new byte[] {4,4,4 }, },
                new RPI() { Key = new byte[] {5,5,5, }, },
            };
            CBStorage.AppendRPIs(rpis2);
            items = CBStorage.LoadRPIs();
            Assert.Equal(5, items.Count);

            Assert.Equal(1, items[0].Key[0]);
            Assert.Equal(5, items[4].Key[0]);
        }


        [Fact]
        public void TestInitTEK()
        {
            CBStorage.InitTEK();
            var items = CBStorage.LoadTEK();

            Assert.NotNull(items);
            Assert.Empty(items);
        }

        [Fact]
        public void TestSaveTEK()
        {
            CBStorage.InitTEK();
            var tek = new TEK
            {
                Key = new byte[] { 1,1,1 },
                RollingStartIntervalNumber = 100,
                RollingPeriod = 144,
                TransmissionRiskLevel = 4,
            };
            CBStorage.AppendTEK(tek);

            var items = CBStorage.LoadTEK();
            Assert.NotNull(items);
            Assert.Single(items);
            Assert.Equal(new byte[] { 1,1,1}, items[0].Key);
            Assert.Equal(144, items[0].RollingPeriod);
            Assert.Equal(4, items[0].TransmissionRiskLevel);
            Assert.Equal(100, items[0].RollingStartIntervalNumber);
        }
        [Fact]
        public void TestSaveTEK2()
        {
            CBStorage.InitTEK();
            var tek = new TEK
            {
                Key = new byte[] { 1,},
                RollingStartIntervalNumber = 100,
            };
            CBStorage.AppendTEK(tek);
            var items = CBStorage.LoadTEK();
            Assert.NotNull(items);
            Assert.Single(items);

            var teks = new List<TEK>
            {
                new TEK() { Key = new byte[]{2}, RollingStartIntervalNumber = 200},
                new TEK() { Key = new byte[]{3}, RollingStartIntervalNumber = 300},
                new TEK() { Key = new byte[]{4}, RollingStartIntervalNumber = 400},
            };
            CBStorage.AppendTEK(teks);
            items = CBStorage.LoadTEK();
            Assert.Equal(4, items.Count);
            Assert.Equal(new byte[] { 1, }, items[0].Key);
            Assert.Equal(new byte[] { 4, }, items[3].Key);
        }
    }
}

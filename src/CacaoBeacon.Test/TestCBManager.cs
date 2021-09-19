using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using OpenCacao.CacaoBeacon;
using System.Security.Cryptography;

namespace CacaoBeacon.Test
{
    public class TestCBManager
    {
        [Fact]
        public void TestMatch()
        {
            
            var manager = new CBManager();
            var reciver = new CBReceiver();

            // TEKを作成
            var tek = CBPack.makeTEK();
            var dt = DateTime.Now;
            var rpi = CBPack.getRPI(tek, dt);
            // 最初のRPIを受信状態にする
            reciver.Recv(rpi, dt, -120);

            // zip をダウンロードして TEK を得る
            // 接触判定する
            manager.TEKs = new List<TEK>() { new TEK { 
                Key = tek, 
                Date = dt   }};
            var match = manager.Detect(reciver.RPIs);

            Assert.Equal(1, match.Count);
            Assert.Equal(tek, match[0].Item1.Key);
            Assert.Equal(rpi, match[0].Item2.Key);
        }

        [Fact]
        public void TestMatch2()
        {
            var manager = new CBManager();
            var reciver = new CBReceiver();

            // TEKを作成
            var tek = CBPack.makeTEK();
            var dt = DateTime.Now;
            var rpi = CBPack.getRPI(tek, dt);
            // 最初のRPIを受信状態にする
            reciver.Recv(rpi, dt, -120);

            // zip をダウンロードして TEK を得る
            var tek2 = CBPack.makeTEK();
            // 異なるTEKで判定する接触判定する
            manager.TEKs = new List<TEK>() { new TEK {
                Key = tek2,
                Date = dt   }};
            var match = manager.Detect(reciver.RPIs);

            // マッチしない
            Assert.Empty(match);
        }
        [Fact]
        public void TestMatch3()
        {
            var manager = new CBManager();
            var reciver = new CBReceiver();

            // TEKを作成
            var tek = CBPack.makeTEK();
            var dt = DateTime.Now;
            var rpi = CBPack.getRPI(tek, dt);
            // 最初のRPIを受信状態にする
            reciver.Recv(rpi, dt, -120);

            // zip をダウンロードして TEK を得る
            // 初期化の日付が異なる場合は、マッチしない
            var dt2 = DateTime.Now.AddDays(5);
            manager.TEKs = new List<TEK>() { new TEK {
                Key = tek,
                Date = dt2   }};
            var match = manager.Detect(reciver.RPIs);

            // マッチしない
            Assert.Empty(match);
        }
    }
}

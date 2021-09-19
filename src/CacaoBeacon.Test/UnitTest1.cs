using System;
using System.Collections.Generic;
using Xunit;

#if false
namespace TestProject1
{
    public class UnitTest1
    {

        // CacaoBeaconServer
        // 送信側（ペリフェラル）
        // 1. 1日の最初にTEKiを生成して保持する
        // 2. TEKiから144個分のRPIlistを作成しておく
        // 3. 時刻に応じて、RPIj を取り出して発信する
        // 4. 発信日とTEKiをワンセットで保持する

        // CacaoBeaconReceiver
        // 受信側（セントラル）
        // 1. 送信側から RPI を受信時刻と同時に保持する。
        // 2. 最初のRPIは、開始時刻としては保持する
        // 3. 連続するRPIは、該当するRPIを見つけて終了時刻として更新して保持する
        // 4. RPI, 開始時刻, 終了時刻でワンセットになる


        // CacaoBeaconManager
        // 受信側でRPIの照合チェック（接触確認）
        // 1. TEKのリストを受信する
        // 2. TEKiから、RPIlistを生成する
        // 3. RPIlist と内部で保持する RPIを照合して、接触確認をチェックする
        // 4. 該当なしの場合は、接触確認なし
        // 5. 該当ありの場合は、接触確認あり
        // 6. TEKiからmetadataを復号する
        // 7. 連続するRPI（開始時刻、終了時刻）を利用して、接触時間を確定する
        // 8. 接触時間により、リスク値を確定する？

        public class CacaoBeaconServer
        {
            public void InitTEK(byte[] tek = null) { }
            public TEK TEK { get; }
            public List<TEK> TEKs { get; }
            public byte[] PRI { get; }
            public List<byte[]> PRIs { get; }
            public DateTime Today { get; set; }

            public void Send(byte[] rpi = null) { }
        }

        public class TEK
        {
            public byte[] Key { get; }
            public DateTime Date;           // 生成日
        }

        public class CacaoBeaconReceiver
        {
            public void Recv(byte[] pri, DateTime? time = null) { }
            public List<RPI> RPIs { get; }
        }

        public class RPI
        {
            public byte[] Key { get; }
            public DateTime StartTime { get; } // 開始時刻
            public DateTime EndTime { get; }   // 終了時刻
        }

        public class CacaoBeaconManager
        {
            public void Download() { }
            public List<TEK> TEKs { get; }
            public void Detect( List<RPI> pris ) { }
            public List<Result> GetSummary() {
                return null;
            }
        }


        public class Result
        {
            public TEK TEK { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public TimeSpan Time { get; }   // 接触時間
            public int Risk { get; }        // リスク値
            public int RSSI { get; }        // 距離（電波強度）

        }

        [Fact]
        public void Test1()
        {
            var cbserver = new CacaoBeaconServer();
            // TEKを生成
            cbserver.InitTEK();
            // TEKを確認
            byte[] tek = cbserver.TEK.Key;
            // 過去のTEKの一覧を取得
            var lst = cbserver.TEKs;
            // 現時刻でのRPIを取得
            var rpi = cbserver.PRI;
            // 当日の144個のRPIを取得
            var rpis = cbserver.PRIs;
            // 当日を取得（テスト用に当日が変更できる）
            var day = cbserver.Today;

            // RPIを発信する
            cbserver.Send();
            // 10分経ったら RPI を切り替える
        }

        [Fact]
        public void Test2()
        {
            var cbreceiver = new CacaoBeaconReceiver();

            // 外部でRPIを受信した
            byte[] pri = new byte[16];
            cbreceiver.Recv(pri);
            // 受信したRPIのリスト
            var lst = cbreceiver.RPIs;

            // 同じRPIを受信すると、終了時刻が更新される
            cbreceiver.Recv(pri);
            RPI pri0 = cbreceiver.RPIs.Find(t => t.Key == pri);
            DateTime start = pri0.StartTime;
            DateTime end = pri0.EndTime;


        }

        [Fact]
        public void Test3()
        {
            var cbmanager = new CacaoBeaconManager();
            var cbreceiver = new CacaoBeaconReceiver();

            // zip ファイルをダウンロードして TEKのリストを得る
            cbmanager.Download();
            var teks = cbmanager.TEKs;

            // 受信したRPIと照合する
            cbmanager.Detect(cbreceiver.RPIs);
            // 診断結果を得る
            var result = cbmanager.GetSummary();

            foreach ( var it in result )
            {
                var tek = it.TEK;
                var date = it.StartTime;        // 接触した日時
                var time = it.Time;             // 接触時間
                var rssi = it.RSSI;             // 接触距離（最も近づいた距離）
            }

            // 閾値を見て、通知を出すかどうか決める

        }
    }
}
#endif
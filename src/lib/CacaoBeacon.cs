using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OpenCacao.CacaoBeacon
{
    /// <summary>
    ///  Rotating Proximity Identifier
    /// </summary>
    public class RPI
    {
        public byte[] Key { get; set; }
        public DateTime StartTime { get; set; } // 開始時刻
        public DateTime EndTime { get; set; }   // 終了時刻
        public short RSSI_min { get; set; }     // 電波強度(dBm) 最小（遠い）
        public short RSSI_max { get; set; }     // 電波強度(dBm) 最大（近い）
        public ulong MAC { get; set; }          // MAC アドレス
        public string ToKeyString()
        {
            return BitConverter.ToString(this.Key).Replace("-", "").ToLower();
        }
        public string toMacString()
        {
            return string.Join(":",
                        BitConverter.GetBytes(this.MAC).Reverse()
                        .Select(b => b.ToString("X2"))).Substring(6);
        }
    }

    /// <summary>
    ///  Temporary Exposure Key 
    /// </summary>
    public class TEK
    {
        public byte[] Key { get; set; }
        public int RollingStartIntervalNumber { get; set; }
        public int TransmissionRiskLevel { get; set; }
        public int RollingPeriod { get; set; }

        public DateTime Date
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(RollingStartIntervalNumber * 600);
            }
            set
            {
                if ( value.Kind == DateTimeKind.Local )
                {
                    value = value.ToUniversalTime();
                }
                // UTCの正午に正規化する
                var utc = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
                RollingStartIntervalNumber = 
                    (int)(new DateTimeOffset(utc).ToUnixTimeSeconds() / 600);
            }
        }
        public string ToKeyString()
        {
            return BitConverter.ToString(this.Key).Replace("-", "").ToLower();
        }
    }


    public class CBReceiver
    {
        public List<RPI> RPIs { get; } = new List<RPI>();
        public CBStorage Storage { get; set; } = null;

        /// <summary>
        /// 受信した RPI を内部で保持する
        /// 
        /// 新しい RPI の場合は、新規に RPI を作って追加する
        /// 連続した RPI を受信したときは、EndTime を更新する。
        /// これにより、接触時間を秒単位（Beaconの発信単位）で計測できる
        /// 
        /// TODO: 発信時の電波強度のため MetaData も保存
        /// </summary>
        /// <param name="rpi">Rotating Proximity Identifier</param>
        /// <param name="time">受信時刻</param>
        public void Recv(byte[] rpi, DateTime time, short rssi, ulong mac = 0)
        {
            foreach (var it in this.RPIs)
            {
                if ( it.Key.SequenceEqual(rpi) == true && it.MAC == mac )
                {
                    it.EndTime = time;
                    if (it.RSSI_min > rssi) it.RSSI_min = rssi;
                    if (it.RSSI_max < rssi) it.RSSI_max = rssi;
                    this.Storage?.Update(it);
                    return;
                }
            }
            var item = new RPI
            {
                Key = rpi,
                StartTime = time,
                EndTime = time,
                RSSI_min = rssi,
                RSSI_max = rssi,
                MAC = mac,
            };
            this.RPIs.Add(item);
            this.Storage?.Add(item);
        }
        public void Recv(byte[] rpi)
        {
            this.Recv(rpi, DateTime.Now, 0);
        }

        /// <summary>
        /// ストレージから復元
        /// </summary>
        public void LoadStorage()
        {
            if ( this.Storage != null )
            {
                this.RPIs.Clear();
                this.RPIs.AddRange(this.Storage.RPI);
            }
        }
    }

    /// <summary>
    /// RPI, TEK を永続化するためのストレージクラス
    /// </summary>
    public  abstract class CBStorage
    {
        /// <summary>
        /// ストレージを消去してリセットする
        /// </summary>
        public virtual void Reset() { }
        /// <summary>
        /// ひとつの RPI を更新する
        /// </summary>
        /// <param name="item"></param>
        public virtual void Update(RPI item) { }
        /// <summary>
        /// ひとつの RPI を追加する
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(RPI item) { }
        /// <summary>
        /// 複数の RPI を追加する
        /// </summary>
        /// <param name="items"></param>
        public virtual void AddRange(List<RPI> items) { }
        /// <summary>
        /// ひとつの TEK を追加する
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add( TEK item) { }
        /// <summary>
        /// ストレージから RPI のリストを取得する
        /// </summary>
        public virtual List<RPI> RPI => new List<RPI>();
        /// <summary>
        /// ストレージから TEK のリストを取得する
        /// </summary>
        public virtual List<TEK> TEK => new List<TEK>();
    }


    public class CBManager
    {
        public List<TEK> TEKs { get; set; }
        public List<(TEK,RPI)> match { get; set; }

        /// <summary>
        /// 接触判定をする
        /// </summary>
        /// <param name="pris"></param>
        public List<(TEK, RPI)> Detect(List<RPI> rpis) {

            this.match = new List<(TEK,RPI)>();
            foreach ( var tek in this.TEKs )
            {
                var result = Detect(tek, rpis);
                this.match.AddRange(result);
            }
            // 結果は match で返す
            return match;
        }
        /// <summary>
        /// 接触判定をする
        /// </summary>
        /// <param name="pris"></param>
        public static List<(TEK, RPI)> Detect(TEK tek, List<RPI> rpis)
        {
            var match = new List<(TEK, RPI)>();
            // TEK から RPIs を生成する
            var RPIs = CBPack.makeRPIs(tek.Key, tek.RollingStartIntervalNumber);
            // 生成した RPIs が受信した rpis とマッチするか調べる
            var q = from x in RPIs
                    from y in rpis
                    where x.SequenceEqual(y.Key) == true
                    select y;

            var lst = q.ToList();
            if (lst.Count > 0)
            {
                lst.ForEach(rpi => match.Add((tek, rpi)));
            }
            // 結果は match で返す
            return match;
        }
    }

    /// <summary>
    /// Exposure Notification の暗号化クラス
    /// </summary>
    public class CBPack
    {
        // public static byte[] RPIKi { get; set; }
        // public static List<byte[]> RPIList = new List<byte[]>();

        static byte[] EN_PRIK = stobytes("EN-RPIK");
        static byte[] EN_RPI = stobytes("EN-RPI");

        /// <summary>
        /// ランダムなTEKを作成する
        /// </summary>
        /// <returns></returns>
        public static byte[] makeTEK()
        {
            var rnd = new Random();
            byte[] tek = new byte[16];
            for( int i=0; i<tek.Length; i++ )
            {
                tek[i] = (byte)rnd.Next(256);
            }
            return tek;
        }

        /// <summary>
        /// TEKと現在時刻からRPIを取得する
        /// </summary>
        /// <param name="tek"></param>
        public static byte[] getRPI(byte[] tek, DateTime now )
        {
            // ローカルタイムの場合は UTC に直す
            if ( now.Kind == DateTimeKind.Local )
            {
                now = now.ToUniversalTime();
            }
            var today = new DateTime(now.Year, now.Month, now.Day, 0,0,0,DateTimeKind.Utc);
            int rolling_start_interval_number = (int)new DateTimeOffset(today).ToUnixTimeSeconds() / 600;
            var rpis = makeRPIs( tek, rolling_start_interval_number);
            var n = (int)((now - today).TotalSeconds) / 600;
            return rpis[n];
        }

        /// <summary>
        /// TEK と rolling_start_interval_number から
        /// 144 個の RPI を生成する
        /// </summary>
        /// <param name="tek"></param>
        /// <param name="rolling_start_interval_number"></param>
        /// <returns></returns>
        public static List<byte[]> makeRPIs(byte[] tek, int rolling_start_interval_number)
        {
            if (tek == null)
                throw new ArgumentNullException(nameof(tek));
            if (tek.Length != 16)
                throw new ArgumentException($"The length of TEK must be equal 16 bytes.");

#if __ANDROID__
            byte[] HKDF( byte[] ikm, byte[] salt, byte[] info, int len )
            {
                var hkdf = new AronParker.Hkdf.Hkdf(HashAlgorithmName.SHA256);
                var actualPrk = hkdf.Extract(ikm, salt);
                var actualOkm = hkdf.Expand(actualPrk, len, info);
                return actualOkm;
            }
            var RPIKi = HKDF(tek, null, EN_PRIK, 16);
#else
            // .net5 の場合は HKDF クラスがある
            var RPIKi = HKDF.DeriveKey(HashAlgorithmName.SHA256, tek, 16, null, EN_PRIK);
#endif

            int ENINi = (int)rolling_start_interval_number;

            var aes = new AesCryptoServiceProvider()
            {
                BlockSize = 128,
                KeySize = 128,
                IV = new byte[16],// IV は null 固定
                Key = RPIKi,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            /// PaddedDatajを144個分まとめて暗号化する
            /// その後で144分割して16バイトにする
            /// 最後に16バイト余るが無視でよいらしい
            // PaddedDataj[0..5] = "EN-PRI"
            // PaddedDataj[6..11] = 0x0
            // PaddedDataj[12..15] = ENINj
            var PaddedDataj = new List<byte>();
            for (int i = 0; i < 144; i++)
            {
                PaddedDataj.AddRange(EN_RPI);
                PaddedDataj.AddRange(new byte[6]);
                PaddedDataj.AddRange(BitConverter.GetBytes(ENINi + i));
            }

            byte[] AES(byte[] data)
            {
                var encrypt = aes.CreateEncryptor();
                var encrypted = encrypt.TransformFinalBlock(data, 0, data.Length);
                return encrypted;
            };
            byte[] RPIij = AES(PaddedDataj.ToArray());
            // 144分割する
            var lst = new List<byte[]>();
            for (int i = 0; i < 144; i++)
            {
                byte[] RPIj = RPIij[(16 * i)..(16 * (i + 1))];
                int ENINj = ENINi + i;
                lst.Add(RPIj);
            }
            return lst;
        }

        static byte[] stobytes(string s)
        {
            var data = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                data[i] = (byte)s[i];
            }
            return data;
        }
    }
}

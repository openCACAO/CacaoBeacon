using Google.Protobuf;
using OpenCacao.CacaoBeacon;
using Proto = OpenCacao.CacaoBeacon.Proto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Diagnostics;

namespace probetek
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // JSONの一覧を取得
                displayZipList();
                return;
            }
            if (args.Length == 1 && (args[0] == "-a" || args[0] == "--all")) 
            {
                displayAllExportBin();
                return;
            }
            if (args.Length == 2 && (args[0] == "-a" || args[0] == "--all"))
            {
                string folder = args[1];
                saveAllExportBin(folder);
                return;
            }
            if ( args.Length == 1 && args[0] == "--update-tek" )
            {
                updateTek();
                return;
            }
            if (args.Length == 2 && args[0] == "--update-rpi" )
            {
                updateRpi(args[1]);
                return;
            }
            if (args.Length == 2 && args[0] == "--detect")
            {
                string sqlite = args[1];
                detectTek(sqlite);
                return;
            }
            if (args.Length == 3 && args[0] == "--detect")
            {
                string sqlite = args[1];
                string folder = args[2];
                detectTek(sqlite, folder);
                return;
            }



            if ( args.Length == 1 )
            {
                displayExportBin(args[0]);
                return;
            }
            Usage();
        }

        private static void Usage()
        {
            Console.WriteLine(@"
$ probetek            : list.json を取得し、zip 一覧を表示
$ probetek [zip-url]  : 指定した zip をダウンロードし、TEK 一覧を表示
$ probetek [-a|--all] : すべての zip をダウンロードし、TEK 一覧を表示
$ probetek [-a|--all] [folder] : すべての zip をダウンロードし、指定フォルダーに保存
$ probetek [--update-tek] : すべての zip をダウンロードし、TEKからRPIを生成してデータベースに登録
$ probetek [--update-rpi] [path] : CacaoBeconMonitor の SQLite データをデータベースに登録
$ probetek [--detect] [sqlite] : zip をダウンロードして、SQLite データと照合処理
$ probetek [--detect] [sqlite] [folder]: 指定フォルダーの zip を使い、SQLite データと照合処理

");

        }

        /// <summary>
        /// ZIPの一覧を取得
        /// </summary>
        private static void displayZipList()
        {
            var url = "https://covid19radar-jpn-prod.azureedge.net/c19r/440/list.json";
            var cl = new HttpClient();
            var response = cl.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var lst = JsonSerializer.Deserialize<ZipTekList>(json);
            int n = 1;
            Console.WriteLine($"count: {lst.Count}");
            foreach (var it in lst)
            {
                Console.WriteLine($"{n:0}: {it.createdDate} {it.url}");
                n++;
            }

        }
        /// <summary>
        /// 指定のZIPをダウンロードして export.bin を取得
        /// </summary>
        /// <param name="url"></param>
        private static void displayExportBin( string url )
        {
            var data = ExposureNotification.GetExportBin(url).Result;
            var teke = Proto.TemporaryExposureKeyExport.Parser.ParseFrom(data);
            List<TemporaryExposureKey> teks = ExposureNotification.ConvertTEK(teke);
            Console.WriteLine($"TEK export");
            Console.WriteLine($"StartTimestamp: {teke.StartTimestamp} " +
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(teke.StartTimestamp));
            Console.WriteLine($"EndTimestamp: {teke.EndTimestamp} " +
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(teke.EndTimestamp));
            Console.WriteLine($"Region: {teke.Region}");
            Console.WriteLine($"BatchNum: {teke.BatchNum}");
            Console.WriteLine($"BatchSize: {teke.BatchSize}");

            Console.WriteLine($"SignatureInfo.Count: {teke.SignatureInfos.Count}");
            foreach (var sig in teke.SignatureInfos)
            {
                Console.WriteLine($" VerificationKeyVersion: {sig.VerificationKeyVersion}");
                Console.WriteLine($" VerificationKeyId: {sig.VerificationKeyId}");
                Console.WriteLine($" SignatureAlgorithm: {sig.SignatureAlgorithm}");
            }
            Console.WriteLine($"Keys.Count: {teke.Keys.Count}");
            int n = 1;
            foreach (TemporaryExposureKey tek in teks)
            {
                Console.WriteLine($"{n}: " + BitConverter.ToString(tek.Key).Replace("-", "").ToLower());
                Console.WriteLine($" TransmissionRiskLevel: {tek.TransmissionRiskLevel}");
                Console.WriteLine($" RollingStartIntervalNumber: {tek.RollingStartIntervalNumber} {tek.Date}");
                Console.WriteLine($" RollingPeriod: {tek.RollingPeriod}");
                n++;
            }
        }

        /// <summary>
        /// すべてのZIPをダウンロードして export.bin を取得
        /// </summary>
        /// <param name="url"></param>
        private static void displayAllExportBin()
        {
            var url = "https://covid19radar-jpn-prod.azureedge.net/c19r/440/list.json";
            var cl = new HttpClient();
            var response = cl.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var lst = JsonSerializer.Deserialize<ZipTekList>(json);
            Console.Write("download zip");
            List<TemporaryExposureKey> teks = new List<TemporaryExposureKey>();
            foreach (var it in lst)
            {
                Console.Write(".");
                var data = ExposureNotification.GetExportBin(it.url).Result;
                var teke = Proto.TemporaryExposureKeyExport.Parser.ParseFrom(data);
                teks.AddRange( ExposureNotification.ConvertTEK(teke));
            }
            Console.WriteLine("");
            int n = 0;

            /// 時刻でソートする
            teks = teks.OrderBy(t => t.RollingStartIntervalNumber).ToList();
            foreach ( var it in teks )
            {
                n++;
                Console.WriteLine($"{n} {it.RollingStartIntervalNumber} {it.Date} {it.ToKeyString()}");
            }
            Console.WriteLine($"");
            Console.WriteLine($"ZIP count is {lst.Count}");
            Console.WriteLine($"TEK count is {teks.Count}");
        }

        /// <summary>
        /// すべてのZIPをダウンロードして、指定フォルダーに保存
        /// </summary>
        /// <param name="url"></param>
        private static void saveAllExportBin( string folder )
        {
            var url = "https://covid19radar-jpn-prod.azureedge.net/c19r/440/list.json";
            var cl = new HttpClient();
            var response = cl.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var lst = JsonSerializer.Deserialize<ZipTekList>(json);
            Console.Write($"download zip to {folder}");
            // フォルダーを作成する

            if ( !System.IO.Directory.Exists(folder))
            {
                var di = System.IO.Directory.CreateDirectory(folder);
            }
            // フォルダーに zip を保存する
            foreach (var it in lst)
            {
                // Console.Write(".");
                string path = System.IO.Directory.GetCurrentDirectory() + "\\"
                    + folder + "\\"
                    + System.IO.Path.GetFileName(it.url);
                Console.WriteLine( path );

                response = cl.GetAsync(it.url).Result;
                var zipdata = response.Content.ReadAsByteArrayAsync().Result;
                System.IO.File.WriteAllBytes(path, zipdata);
            }
            Console.WriteLine($"ZIP count is {lst.Count}");
        }

        partial class ZipTekList : List<ZipTek> { }
        private class ZipTek
        {
            public string region { get; set; }
            public string url { get; set; }
            public ulong created { get; set; }
            public DateTime createdDate =>
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(created);
        }

        /// <summary>
        /// 仮実装
        /// TEKをダウンロードして、144個のRPIに直してデータベースに保存する
        /// </summary>
        private static void updateTek()
        {
            Console.WriteLine("download TEK zip & update to database");
            Console.WriteLine($"Download TEK zip from server");
            var teks = ExposureNotification.DownloadBatchAsync().Result;
            Console.WriteLine($"Count of TEK: {teks.Count}");

            var items = new List<ExportRotatingProximityIdentifier>();
            foreach (var tek in teks)
            {
                var rpis = CBPack.makeRPIs(tek.Key, tek.RollingStartIntervalNumber);
                int n = 0;
                foreach (var it in rpis)
                {
                    items.Add(new ExportRotatingProximityIdentifier()
                    {
                        TEK = tek.Key,
                        RPI = it,
                        RollingStartIntervalNumber = tek.RollingStartIntervalNumber,
                        StartDate = tek.Date,
                        RpiDate = tek.Date.AddMinutes(10 * n),
                    });
                    n++;
                }
            }
            Console.WriteLine($"Count of EXRPI: {items.Count}");
            Console.WriteLine($"Insert EXRPI");
            var context = new CBContextSQLServer();
            context.Database.ExecuteSqlRaw("delete from [EXRPI]");
            context.EXRPI.AddRange(items);
            context.SaveChanges();
            Console.WriteLine($"Insert EXRPI complete");

        }

        /// <summary>
        /// TEKから展開したRPIをデータベースに上げるとパンクするので
        /// メモリ上で日単位で照合する
        /// ==================================================================
        /// 	                       日単位		メモリ量	
        /// TEK	       500,000 	件	   35,714 			
        /// RPI	    72,000,000 		5,142,857 		78 MB
        /// 取得RPI	     1,600 	件
        /// ==================================================================
        /// </summary>
        private static void detectTek(string sqlite, string folder = "")
        {
            /// 1. json から zip をダウンロードする
            /// 1.1 フォルダー指定があれば、指定フォルダーの zip を使う
            /// 2. 解凍して teks を取得する（時系列でソートする）
            List<TemporaryExposureKey> teks = new List<TemporaryExposureKey>();
            if (folder == "" )
            {
                var url = "https://covid19radar-jpn-prod.azureedge.net/c19r/440/list.json";
                var cl = new HttpClient();
                var response = cl.GetAsync(url).Result;
                var json = response.Content.ReadAsStringAsync().Result;
                var lst = JsonSerializer.Deserialize<ZipTekList>(json);
                Console.Write("download zip");
                foreach (var it in lst)
                {
                    Console.Write(".");
                    var data = ExposureNotification.GetExportBin(it.url).Result;
                    var teke = Proto.TemporaryExposureKeyExport.Parser.ParseFrom(data);
                    teks.AddRange(ExposureNotification.ConvertTEK(teke));
                }
            } 
            else
            {
                var dir = System.IO.Directory.GetCurrentDirectory();
                var files = System.IO.Directory.GetFiles(folder);
                foreach ( var file in files)
                {
                    var data = ExposureNotification.GetExportFile(file);
                    var teke = Proto.TemporaryExposureKeyExport.Parser.ParseFrom(data);
                    teks.AddRange(ExposureNotification.ConvertTEK(teke));
                }
            }
            /// 時刻でソートする
            teks = teks.OrderBy(t => t.RollingStartIntervalNumber).ToList();
            Console.WriteLine($"");
            Console.WriteLine($"TEK count is {teks.Count}");
            /// 3. 接触RPIs を SQLite から読み込んでおく
            Console.WriteLine("load SQLite");
            CBStorageSQLite.PATH_DB = sqlite;
            CBStorageSQLite.CBContext context = new CBStorageSQLite.CBContext();
            var items = context.RPI.ToList();
            Console.WriteLine($"count of RPI : {items.Count}");

            /// 3.1 接触RPIの日付(UTC)の範囲だけ繰り返す
            var days = items.Select(t => t.StartTime.UtcDateTime.Date).Distinct().ToList();
            /// 4. teks から日付(UTC)で絞り込む
            foreach ( var day in days)
            {
                Console.WriteLine($"check {day}");
                var itemsRpi = items.Where(t => t.StartTime.UtcDateTime.Date == day).ToList();
                Console.WriteLine($"itemsRpi.Count {itemsRpi.Count}");
                var dayTeks = teks.Where(t => t.Date == day).ToList();
                Console.WriteLine($"dayTeks.Count {dayTeks.Count}");
                var itemsTek = new List<ExportRotatingProximityIdentifier>();
                /// 6. 接触RPIの日付(UTC)分だけ繰り返す
                foreach ( var tek in dayTeks )
                {
                    var rpis = CBPack.makeRPIs(tek.Key, tek.RollingStartIntervalNumber);
                    int n = 0;
                    /// 4.1 144個のRPIに展開する
                    foreach (var it in rpis)
                    {
                        itemsTek.Add(new ExportRotatingProximityIdentifier()
                        {
                            TEK = tek.Key,
                            RPI = it,
                            RollingStartIntervalNumber = tek.RollingStartIntervalNumber,
                            StartDate = tek.Date,
                            RpiDate = tek.Date.AddMinutes(10 * n),
                        });
                        n++;
                    }
                }
                /*
                // テスト用
                // 1個だけ itemsRpi に追加しておく
                itemsRpi.Add(new RotatingProximityIdentifier()
                {
                    Id = 100,
                    Key = itemsTek[0].RPI,
                    StartTime = itemsTek[0].StartDate,
                    EndTime = itemsTek[0].StartDate.AddMinutes(1),
                    RssiMin = -99,
                    RssiMax = -20,
                });
                Console.WriteLine($"test rpi key: " + BitConverter.ToString(itemsTek[0].RPI).Replace("-", "").ToLower());
                */

                Console.WriteLine($"itemsTek.Count {itemsTek.Count}");
                var st = new Stopwatch();
                st.Start();
                Console.WriteLine($"経過開始: {DateTime.Now}");
                /// 5. 陽性RPIと接触RPIを突き合わせする
                // itemsRpi と itemsTek を突き合わせる
                var itemsMatch = from rpi in itemsRpi
                                 join tek in itemsTek on rpi.Key equals tek.RPI
                                 select rpi;

                Console.WriteLine($"itemsMatch.Count: {itemsMatch.Count()}");
                st.Stop();
                Console.WriteLine($"経過時間: {st.Elapsed}");

                if (itemsMatch.Count() > 0 )
                {
                    foreach( var rpi in itemsMatch ) {
                        Console.WriteLine($"detect: " + BitConverter.ToString(rpi.Key).Replace("-", "").ToLower());
                    }
                }
            }
        }

        /// <summary>
        /// 仮実装
        /// SQLiteのデータを RPI テーブルにアップロード
        /// </summary>
        /// <param name="path"></param>
        private static void updateRpi( string path )
        {
            Console.WriteLine("RPI update from SQLite to SQL Server");
            Console.WriteLine("load SQLite");
            CBStorageSQLite.PATH_DB = path;
            CBStorageSQLite.CBContext context = new CBStorageSQLite.CBContext();
            var items = context.RPI.ToList();
            Console.WriteLine($"count of RPI : {items.Count}");
            // SQL Server へ挿入
            Console.WriteLine("save SQL Server");
            CBContextSQLServer outcontext = new CBContextSQLServer();
            foreach (var it in items)
            {
                it.Id = 0;
                outcontext.RPI.Add(it);
            }
            outcontext.SaveChanges();
            Console.WriteLine($"Insert RPI complete");
        }
    }
}

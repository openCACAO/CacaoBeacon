using Google.Protobuf;
using OpenCacao.CacaoBeacon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
$ probetek [--update-tek] : すべての zip をダウンロードし、TEKからRPIを生成してデータベースに登録
$ probetek [--update-rpi] [path] : CacaoBeconMonitor の SQLite データをデータベースに登録

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
            var teke = TemporaryExposureKeyExport.Parser.ParseFrom(data);
            List<TEK> teks = ExposureNotification.ConvertTEK(teke);
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
            foreach (TEK tek in teks)
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
            List<TEK> teks = new List<TEK>();
            foreach (var it in lst)
            {
                Console.Write(".");
                var data = ExposureNotification.GetExportBin(it.url).Result;
                var teke = TemporaryExposureKeyExport.Parser.ParseFrom(data);
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

            var items = new List<EXRPI>();
            foreach (var tek in teks)
            {
                var rpis = CBPack.makeRPIs(tek.Key, tek.RollingStartIntervalNumber);
                int n = 0;
                foreach (var it in rpis)
                {
                    items.Add(new EXRPI()
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

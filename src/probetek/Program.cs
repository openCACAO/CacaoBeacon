using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;


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

            // 指定のZIPをダウンロードして export.bin を取得
            var url = args[0];
            var cl = new HttpClient();
            var response = cl.GetAsync(url).Result;
            var zipdata = response.Content.ReadAsByteArrayAsync().Result;
            File.WriteAllBytes("_export.zip", zipdata);
            var zip = System.IO.Compression.ZipFile.OpenRead("_export.zip");
            var length = zip.GetEntry("export.bin").Length;
            var fs = new BinaryReader(zip.GetEntry("export.bin").Open());
            fs.ReadBytes(12);
            var data = fs.ReadBytes((int)length - 12);
            fs.Close();
            zip.Dispose();
            System.IO.File.Delete("_export.zip");

            // "EK Export v1" を読み飛ばし
            // var data = new byte[fs.Length - 12];
            // fs.Position = 12;
            // fs.Read(data);
            // fs.Close();

            var teke = TemporaryExposureKeyExport.Parser.ParseFrom(data);
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
            foreach (var tek in teke.Keys)
            {
                var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(tek.RollingStartIntervalNumber * 600);
                Console.WriteLine($"{n}: " + BitConverter.ToString(tek.KeyData.ToByteArray()).Replace("-", "").ToLower());
                Console.WriteLine($" TransmissionRiskLevel: {tek.TransmissionRiskLevel}");
                Console.WriteLine($" RollingStartIntervalNumber: {tek.RollingStartIntervalNumber} {dt}");
                Console.WriteLine($" RollingPeriod: {tek.RollingPeriod}");
                n++;
            }


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
        partial class ZipTekList : List<ZipTek> { }
        private class ZipTek
        {
            public string region { get; set; }
            public string url { get; set; }
            public ulong created { get; set; }
            public DateTime createdDate =>
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(created);
        }
    }
}

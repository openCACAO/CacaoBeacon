using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Proto = OpenCacao.CacaoBeacon.Proto;
#if __ANDROID__
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif
using System.Threading.Tasks;

namespace OpenCacao.CacaoBeacon
{
    public class ExposureNotification
    {
        public static string JsonUrl = "https://covid19radar-jpn-prod.azureedge.net/c19r/440/list.json";


        /// <summary>
        /// JSON形式のURLを指定して、TEKのリストを取得する
        /// </summary>
        /// <returns></returns>
        public static async Task<List<TemporaryExposureKey>> DownloadBatchAsync()
        {
            List<TemporaryExposureKey> result = new List<TemporaryExposureKey>();

            // JSONファイルをダウンロードする
            var cl = new HttpClient();
            var response = await cl.GetAsync(JsonUrl);
            var json = await response.Content.ReadAsStringAsync();
#if __ANDROID__
            var zips = JsonConvert.DeserializeObject<ZipTekList>(json);
#else
            var zips = JsonSerializer.Deserialize<ZipTekList>(json);
#endif

            foreach ( var zip in zips )
            {
                var data = await GetExportBin(zip.url);
                var teke = Proto.TemporaryExposureKeyExport.Parser.ParseFrom(data);
                var teks = ConvertTEK(teke);
                result.AddRange(teks);
            }
            return result;
        }

        /// <summary>
        /// ZIPのURLを指定して、TEKのリストを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<List<TemporaryExposureKey>> DownloadBatchAsync( string url )
        {
            byte[] data = await GetExportBin(url);
            var teke = Proto.TemporaryExposureKeyExport.Parser.ParseFrom(data);
            return ConvertTEK(teke);
        }


        /// <summary>
        /// TemporaryExposureKeyExport から List<TEK>に変換
        /// </summary>
        /// <param name="teke"></param>
        /// <returns></returns>
        public static List<TemporaryExposureKey> ConvertTEK(Proto.TemporaryExposureKeyExport teke)
        {
            var result = new List<TemporaryExposureKey>();
            foreach (var tek in teke.Keys)
            {
                result.Add(new TemporaryExposureKey()
                {
                    Key = tek.KeyData.ToByteArray(),
                    RollingStartIntervalNumber = tek.RollingStartIntervalNumber,
#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                    TransmissionRiskLevel = tek.TransmissionRiskLevel,
#pragma warning restore CS0612 // 型またはメンバーが旧型式です
                    RollingPeriod = tek.RollingPeriod,
                });
            }
            return result;
        }

        /// <summary>
        /// ZIPファイルのURLを指定して export.bin のデータを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetExportBin( string url )
        {
            var cl = new HttpClient();
            var response = await cl.GetAsync(url) ;
            var zipdata = await response.Content.ReadAsByteArrayAsync() ;
            byte[] data;
            using (var mem = new MemoryStream(zipdata))
            {
                using (var zip = new System.IO.Compression.ZipArchive(mem))
                {
                    var length = zip.GetEntry("export.bin").Length;
                    using (var fs = new BinaryReader(zip.GetEntry("export.bin").Open()))
                    {
                        fs.ReadBytes(12);
                        data = fs.ReadBytes((int)length - 12);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// ZIPファイルのパスを指定して export.bin のデータを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static byte[] GetExportFile(string path)
        {
            var zipdata = System.IO.File.ReadAllBytes(path);
            byte[] data;
            using (var mem = new MemoryStream(zipdata))
            {
                using (var zip = new System.IO.Compression.ZipArchive(mem))
                {
                    var length = zip.GetEntry("export.bin").Length;
                    using (var fs = new BinaryReader(zip.GetEntry("export.bin").Open()))
                    {
                        fs.ReadBytes(12);
                        data = fs.ReadBytes((int)length - 12);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// サーバーからダウンロードした TEK のリストと
        /// スマホ内部で保持する RPI のリストを照合させる
        /// 
        /// TODO: そのまま TEK, RPI のセットでは分析しずらいので
        /// 元の EN API のように Summary を出力させる
        /// マッチの検出は別に Configure が必要
        /// </summary>
        /// <param name="teks"></param>
        /// <param name="rpis"></param>
        /// <returns></returns>
        public static List<(TemporaryExposureKey, RotatingProximityIdentifier)> FetchExposureKeyAsync(
            List<TemporaryExposureKey> teks, 
            List<RotatingProximityIdentifier> rpis )
        {
            var manager = new CBManager();
            manager.TEKs = teks;
            var result = manager.Detect(rpis);
            return result;
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

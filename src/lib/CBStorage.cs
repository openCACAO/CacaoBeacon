using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
#if __ANDROID__
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif
using System.Threading.Tasks;


namespace OpenCacao.CacaoBeacon
{
    /// <summary>
    /// 発信した TEK と受信した RPIs をストレージに保存するクラス
    /// </summary>
    public class CBStorage
    {
        public const string PATH_RPIS = "cb_rpis.json";
        public const string PATH_TEK = "cb_tek.json";

        /// <summary>
        /// 受信したRPIを追加保存する
        /// </summary>
        /// <param name="rpis"></param>
        public static void AppendRPIs( List<RPI> rpis )
        {
            var items = LoadRPIs();
            items.AddRange(rpis);
            string path = PATH_RPIS;
#if __ANDROID__ || __IOS__
            var json = JsonConvert.SerializeObject(items);
#else
            var json = JsonSerializer.Serialize(items);
#endif
            using (var fs = File. OpenWrite(path))
            {
                using ( var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
        }
        /// <summary>
        /// ストレージからTEKをロードする
        /// </summary>
        /// <returns></returns>
        public static List<RPI> LoadRPIs()
        {
            string path = PATH_RPIS;
            if (File.Exists(path) == false)
            {
                return new List<RPI>();
            } 
            else
            {
                using ( var fs = File.OpenRead(path))
                {
                    using ( var sr = new StreamReader(fs))
                    {
                        string json = sr.ReadToEnd();

#if __ANDROID__ || __IOS__
                        var rpis = JsonConvert.DeserializeObject<List<RPI>>(json);
#else
                        var rpis = JsonSerializer.Deserialize<List<RPI>>(json);
#endif
                        return rpis;
                    }
                }
            }
        }
        /// <summary>
        /// ストレージからクリアして初期化する
        /// </summary>
        public static void InitRPIs()
        {
            string path = PATH_RPIS;

            if ( File.Exists( path ) == true )
            {
                File.Delete(path);
            }
        }
        

        public static void AppendTEK( TEK tek )
        {
            AppendTEK(new List<TEK> { tek });
        }

        public static void AppendTEK(List<TEK> teks)
        {
            var items = LoadTEK();
            items.AddRange(teks);
            string path = PATH_TEK;
#if __ANDROID__ || __IOS__
            var json = JsonConvert.SerializeObject(items);
#else
            var json = JsonSerializer.Serialize(items);
#endif
            using (var fs = File.OpenWrite(path))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }

        }
        public static List<TEK> LoadTEK()
        {
            string path = PATH_TEK;
            if (File.Exists(path) == false)
            {
                return new List<TEK>();
            }
            else
            {
                using (var fs = File.OpenRead(path))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string json = sr.ReadToEnd();

#if __ANDROID__ || __IOS__
                        var teks = JsonConvert.DeserializeObject<List<TEK>>(json);
#else
                        var teks = JsonSerializer.Deserialize<List<TEK>>(json);
#endif
                        return teks;
                    }
                }
            }
        }
        /// <summary>
        /// ストレージからクリアして初期化する
        /// </summary>
        public static void InitTEK()
        {
            string path = PATH_TEK;

            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
        }

    }

}

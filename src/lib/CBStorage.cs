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
        public void SaveRPIs( List<RPI> rpis )
        {

        }
        public List<RPI> LoadRPIs()
        {
            return new List<RPI>();
        }

        public void SaveTEK( TEK tek )
        {

        }

        public void SaveTEK(List<TEK> teks)
        {

        }
        public List<TEK> LoadTEK()
        {
            return new List<TEK>();
        }

    }

}

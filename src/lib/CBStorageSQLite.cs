using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCacao.CacaoBeacon
{

    public class CBStorageSQLite : CBStorage
    {
        public const string PATH_DB = "cacaodb.sqlite3";
        private CBContext _context;

        public static CBStorageSQLite Create( bool reset = false)
        {
            if ( reset == false )
            {
                CBContext context = CBStorageSQLite.CreateContext();
                return new CBStorageSQLite() { _context = context };
            } 
            else
            {
                var o = new CBStorageSQLite();
                o.Reset();
                return o;
            }
        }
        
        public CBStorageSQLite() { 
            _context = CBStorageSQLite.CreateContext(); 
        }
        protected static CBContext CreateContext()
        {
#if __ANDROID__
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + PATH_DB;
#else
            string path = PATH_DB;
#endif
            CBContext context;
            if (File.Exists(path) == false)
            {
                context = new CBContext();
                context.Database.ExecuteSqlRaw(@"
CREATE TABLE RPI (
    ID    INTEGER NOT NULL,
    Key   TEXT NOT NULL,
    StartTime TEXT NOT NULL,
    EndTime   TEXT NOT NULL,
    RSSI_min  INTEGER NOT NULL,
    RSSI_max  INTEGER NOT NULL,
    MAC   TEXT NOT NULL,
    PRIMARY KEY(ID AUTOINCREMENT)
);
CREATE TABLE TEK (
    ID    INTEGER NOT NULL,
    Key   TEXT NOT NULL,
    RollingStartIntervalNumber  INTEGER NOT NULL,
    TransmissionRiskLevel  INTEGER NOT NULL,
    RollingPeriod  INTEGER NOT NULL,
    PRIMARY KEY(ID AUTOINCREMENT)
)
");
            }
            else
            {
                context = new CBContext();
            }
            return context;
        }

        /// <summary>
        /// ストレージを消去してリセットする
        /// </summary>
        public override void Reset()
        {
            if ( _context != null )
            {
                _context.Dispose();
                _context = null;
            }

#if __ANDROID__
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + PATH_DB;
#else
            string path = PATH_DB;
#endif

            if (File.Exists(path) == true)
            {
                System.IO.File.Delete(path);
            }
            _context = CreateContext();
        }

        /// <summary>
        /// ひとつのRPIを追加する
        /// </summary>
        /// <param name="rpi"></param>
        public override void Add( RPI rpi )
        {
            _context.Add(_RPI.ConverFrom(rpi));
            _context.SaveChanges();
        }
        /// <summary>
        /// 複数のRPIを追加する
        /// </summary>
        /// <param name="rpis"></param>
        public override void AddRange(List<RPI> rpis)
        {
            foreach ( var it in rpis )
            {
                _context.Add(_RPI.ConverFrom(it));
            }
            _context.SaveChanges();
        }
        /// <summary>
        /// RPIを更新する
        /// </summary>
        /// <param name="rpi"></param>
        public override void Update(RPI rpi)
        {
            var item = _context.RPI.FirstOrDefault(t => t.Key == rpi.ToKeyString());
            if (item == null) return;
            var newitem = _RPI.ConverFrom(rpi);
            item.EndTime = newitem.EndTime;
            item.RSSI_max = newitem.RSSI_max;
            item.RSSI_min = newitem.RSSI_min;
            _context.SaveChanges();
        }

        public override List<RPI> RPI
        {
            get
            {
                var items = new List<RPI>();
                foreach (var it in _context.RPI.ToList())
                {
                    items.Add(it.ConvertTo());
                }
                return items;
            }
        }


        public class CBContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
#if __ANDROID__
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + PATH_DB;
#else
            string path = PATH_DB;
#endif
                optionsBuilder.UseSqlite($"Data Source={path}");
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<_RPI>().HasKey(t => t.ID);
                modelBuilder.Entity<_TEK>().HasKey(t => t.ID);

            }
            public DbSet<_RPI> RPI { get; set; }
            public DbSet<_TEK> TEK { get; set; }
        }

        /// <summary>
        /// SQLite用のRPI
        /// </summary>
        public class _RPI
        {
            public int ID { get; set; }
            public string Key { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public int RSSI_min { get; set; }
            public int RSSI_max { get; set; }
            public string MAC { get; set; }

            public static _RPI ConverFrom(RPI rpi)
            {
                return new _RPI()
                {
                    Key = rpi.ToKeyString(),
                    StartTime = rpi.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = rpi.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    RSSI_min = rpi.RSSI_min,
                    RSSI_max = rpi.RSSI_max,
                    MAC = rpi.toMacString()
                };
            }
            public RPI ConvertTo()
            {
                var rpi = new RPI();

                byte[] data = new byte[this.Key.Length / 2];
                var s = this.Key.ToUpper();
                for ( int i=0; i<data.Length; i++ )
                {
                    data[i] = (byte)("0123456789ABCDEF".IndexOf(s[i * 2]) * 16 + "0123456789ABCDEF".IndexOf(s[i * 2 + 1]));
                }
                rpi.Key = data;
                rpi.StartTime = DateTime.Parse(this.StartTime);
                rpi.EndTime = DateTime.Parse(this.EndTime);
                return rpi;
            }
        }

        /// <summary>
        /// SQLite用のTEK
        /// </summary>
        public class _TEK
        {
            public int ID { get; set; }
            public string Key { get; set; }
            public int RollingStartIntervalNumber { get; set; }
            public int TransmissionRiskLevel { get; set; }
            public int RollingPeriod { get; set; }
        
        
            public static _TEK FromTEK( TEK tek )
            {
                return new _TEK()
                {
                    Key = tek.ToKeyString(),
                    RollingStartIntervalNumber = tek.RollingStartIntervalNumber,
                    TransmissionRiskLevel = tek.TransmissionRiskLevel,
                    RollingPeriod = tek.RollingPeriod,
                };
            }
            public TEK ToTEK()
            {
                var tek = new TEK();
                byte[] data = new byte[this.Key.Length / 2];
                var s = this.Key.ToUpper();
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)("0123456789ABCDEF".IndexOf(s[i * 2]) * 16 + "0123456789ABCDEF".IndexOf(s[i * 2 + 1]));
                }
                tek.Key = data;
                tek.RollingStartIntervalNumber = this.RollingStartIntervalNumber;
                tek.TransmissionRiskLevel = this.TransmissionRiskLevel;
                tek.RollingPeriod = this.RollingPeriod;
                return tek;
            }
        }
        
    }
}

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

    public class CBStorageSQLite
    {
        public const string PATH_DB = "cacaodb.sqlite3";
        private static CBContext _context;

        public static void Create()
        {
            if (File.Exists(PATH_DB) == false)
            {
                _context = new CBContext();
                _context.Database.ExecuteSqlRaw(@"
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
                _context = new CBContext();
            }
        }

        public static void Init()
        {
            if ( _context != null )
            {
                _context.Dispose();
                _context = null;
            }
            if (File.Exists(PATH_DB) == true)
            {
                System.IO.File.Delete(PATH_DB);
            }
            // 再作成
            Create();
        }
        public static void InitRPIs()
        {
            Init();
        }
        public static void InitTEK()
        {
            Init();
        }

        public static void AppendRPIs(List<RPI> rpis) 
        { 
            foreach ( var it in rpis )
            {
                _context.Add(_RPI.FromRPI(it));
            }
            _context.SaveChanges();
        }

        public static List<RPI> LoadRPIs() { 
            var items = new List<RPI>();
            foreach ( var it in _context.RPI.ToList())
            {
                items.Add(it.ToRPI());
            }
            return items;
        }

        public static void AppendTEK(TEK tek)
        {
            _context.Add(_TEK.FromTEK(tek));
            _context.SaveChanges();
        }
        public static void AppendTEK(List<TEK> teks)
        {
            foreach (var it in teks)
            {
                _context.Add(_TEK.FromTEK(it));
            }
            _context.SaveChanges();
        }

        public static List<TEK> LoadTEK()
        {
            var items = new List<TEK>();
            foreach (var it in _context.TEK.ToList())
            {
                items.Add(it.ToTEK());
            }
            return items;
        }

        public class CBContext : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite($"Data Source={PATH_DB}");
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

            public static _RPI FromRPI(RPI rpi)
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
            public RPI ToRPI()
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

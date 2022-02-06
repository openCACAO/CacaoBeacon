using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
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
        public static string PATH_DB = "cacaodb.sqlite3";

#if __ANDROID__
        public override string StoragePath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + PATH_DB;
#else
        public override string StoragePath => PATH_DB;
#endif

        private CBContext _context;

        public static CBStorageSQLite Create(bool reset = false)
        {
            if (reset == false)
            {
                var context = new CBContext();
                context.Migrate();
                return new CBStorageSQLite()
                {
                    _context = context,
                };
            }
            else
            {
                var o = new CBStorageSQLite();
                o.Reset();
                return o;
            }
        }
        protected CBStorageSQLite()
        {

        }
        /// <summary>
        /// ストレージを消去してリセットする
        /// </summary>
        public override void Reset()
        {
            if (_context != null)
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
            _context = new CBContext();
            _context.Migrate();
        }

        /// <summary>
        /// ひとつのRPIを追加する
        /// </summary>
        /// <param name="rpi"></param>
        public override void Add(RotatingProximityIdentifier rpi)
        {
            _context.RPI.Add(rpi);
            _context.SaveChanges();
        }
        /// <summary>
        /// 複数のRPIを追加する
        /// </summary>
        /// <param name="rpis"></param>
        public override void AddRange(List<RotatingProximityIdentifier> rpis)
        {
            foreach (var it in rpis)
            {
                _context.Add(it);
            }
            _context.SaveChanges();
        }
        /// <summary>
        /// RPIを更新する
        /// </summary>
        /// <param name="rpi"></param>
        public override void Update(RotatingProximityIdentifier rpi)
        {
            var item = _context.RPI.FirstOrDefault(t => t.Key.SequenceEqual(rpi.Key));
            if (item == null) return;
            item.EndTime = rpi.EndTime;
            item.RssiMax = rpi.RssiMax;
            item.RssiMin = rpi.RssiMin;
            _context.SaveChanges();
        }

        public override List<RotatingProximityIdentifier> RPI
        {
            get
            {
                var items = new List<RotatingProximityIdentifier>();
                foreach (var it in _context.RPI.ToList())
                {
                    items.Add(it);
                }
                return items;
            }
        }

        // マイグレーション
        // Add-Migration Initial
        // Update-Database



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
            public DbSet<RotatingProximityIdentifier> RPI => Set<RotatingProximityIdentifier>();
            public DbSet<TemporaryExposureKey> TEK => Set<TemporaryExposureKey>();
            public DbSet<ExportRotatingProximityIdentifier> EXRPI => Set<ExportRotatingProximityIdentifier>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<RotatingProximityIdentifier>().ToTable("RPI");
                modelBuilder.Entity<TemporaryExposureKey>().ToTable("TEK");
                modelBuilder.Entity<ExportRotatingProximityIdentifier>().ToTable("EXRPI");
            }

            public void Migrate()
            {
                var SQL = @"
CREATE TABLE RPI (
    Id INTEGER NOT NULL CONSTRAINT PK_RPI PRIMARY KEY AUTOINCREMENT,
    Key BLOB NULL,
    Metadata BLOB NULL,
    StartTime TEXT NOT NULL,
    EndTime TEXT NOT NULL,
    RssiMin INTEGER NOT NULL,
    RssiMax INTEGER NOT NULL,
    MAC INTEGER NOT NULL
);
CREATE TABLE TEK (
    Id INTEGER NOT NULL CONSTRAINT PK_TEK PRIMARY KEY AUTOINCREMENT,
    Key BLOB NULL,
    RollingStartIntervalNumber INTEGER NOT NULL,
    TransmissionRiskLevel INTEGER NOT NULL,
    RollingPeriod INTEGER NOT NULL,
    Date TEXT NOT NULL
);
CREATE TABLE EXRPI (
    Id INTEGER NOT NULL CONSTRAINT PK_EXRPI PRIMARY KEY AUTOINCREMENT,
    TEK BLOB NULL,
    RPI BLOB NULL,
    RollingStartIntervalNumber INTEGER NOT NULL,
    StartDate TEXT NOT NULL,
    RpiDate TEXT NOT NULL
);
";
                try
                {
                    this.Database.ExecuteSqlRaw(SQL);
                } catch { 
                    // 既に作成済みの場合はエラーを無視する
                }
            }
        }
    }
}

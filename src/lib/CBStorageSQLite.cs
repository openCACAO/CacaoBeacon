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

        public static CBStorageSQLite Create( bool reset = false)
        {
            if ( reset == false )
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
        protected CBStorageSQLite() {
            
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
            _context = new CBContext();
            _context.Migrate();
        }

        /// <summary>
        /// ひとつのRPIを追加する
        /// </summary>
        /// <param name="rpi"></param>
        public override void Add( RPI rpi )
        {
            _context.RPI.Add(rpi);
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
                _context.Add(it);
            }
            _context.SaveChanges();
        }
        /// <summary>
        /// RPIを更新する
        /// </summary>
        /// <param name="rpi"></param>
        public override void Update(RPI rpi)
        {
            var item = _context.RPI.FirstOrDefault(t => t.Key.SequenceEqual(rpi.Key));
            if (item == null) return;
            item.EndTime = rpi.EndTime;
            item.RSSI_max = rpi.RSSI_max;
            item.RSSI_min = rpi.RSSI_min;
            _context.SaveChanges();
        }

        public override List<RPI> RPI
        {
            get
            {
                var items = new List<RPI>();
                foreach (var it in _context.RPI.ToList())
                {
                    items.Add(it);
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

                /// RPI,TEKのテーブルを自動作成する
                MigrationBuilder migrationBuilder = new MigrationBuilder(this.Database.ProviderName);
                migrationBuilder.CreateTable(
                    name: "RPI",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        Key = table.Column<byte[]>(type: "BLOB", nullable: true),
                        Metadata = table.Column<byte[]>(type: "BLOB", nullable: true),
                        StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                        EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                        RSSI_min = table.Column<short>(type: "INTEGER", nullable: false),
                        RSSI_max = table.Column<short>(type: "INTEGER", nullable: false),
                        MAC = table.Column<ulong>(type: "INTEGER", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_RPI", x => x.Id);
                    });

                migrationBuilder.CreateTable(
                    name: "TEK",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        Key = table.Column<byte[]>(type: "BLOB", nullable: true),
                        RollingStartIntervalNumber = table.Column<int>(type: "INTEGER", nullable: false),
                        TransmissionRiskLevel = table.Column<int>(type: "INTEGER", nullable: false),
                        RollingPeriod = table.Column<int>(type: "INTEGER", nullable: false),
                        Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_TEK", x => x.Id);
                    });
            }
            public DbSet<RPI> RPI => Set<RPI>();
            public DbSet<TEK> TEK => Set<TEK>();

            public void Migrate()
            {
#if __ANDROID__
                var SQL = @"
CREATE TABLE RPI (
    Id INTEGER NOT NULL CONSTRAINT PK_RPI PRIMARY KEY AUTOINCREMENT,
    Key BLOB NOT NULL,
    Metadata BLOB NOT NULL,
    StartTime TEXT NOT NULL,
    EndTime TEXT NOT NULL,
    RSSI_min INTEGER NOT NULL,
    RSSI_max INTEGER NOT NULL,
    MAC INTEGER NOT NULL
);
CREATE TABLE TEK(
    Id INTEGER NOT NULL CONSTRAINT PK_TEK PRIMARY KEY AUTOINCREMENT,
    Key BLOB NOT NULL,
    RollingStartIntervalNumber INTEGER NOT NULL,
    TransmissionRiskLevel INTEGER NOT NULL,
    RollingPeriod INTEGER NOT NULL
);
";
                try
                {
                    this.Database.ExecuteSqlRaw(SQL);
                } catch { 
                    // 既に作成済みの場合はエラーを無視する
                }
#else
                this.Database.Migrate();
#endif
            }
        }
    }
}

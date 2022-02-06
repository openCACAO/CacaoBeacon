using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace migratesqlite.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EXRPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TEK = table.Column<byte[]>(type: "BLOB", maxLength: 16, nullable: true),
                    RPI = table.Column<byte[]>(type: "BLOB", maxLength: 16, nullable: true),
                    RollingStartIntervalNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RpiDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXRPI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<byte[]>(type: "BLOB", maxLength: 16, nullable: true),
                    Metadata = table.Column<byte[]>(type: "BLOB", maxLength: 4, nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RssiMin = table.Column<short>(type: "INTEGER", nullable: false),
                    RssiMax = table.Column<short>(type: "INTEGER", nullable: false),
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
                    Key = table.Column<byte[]>(type: "BLOB", maxLength: 16, nullable: true),
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EXRPI");

            migrationBuilder.DropTable(
                name: "RPI");

            migrationBuilder.DropTable(
                name: "TEK");
        }
    }
}

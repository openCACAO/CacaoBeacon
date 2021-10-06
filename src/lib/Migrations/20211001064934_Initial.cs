using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenCacao.CacaoBeacon.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RPI");

            migrationBuilder.DropTable(
                name: "TEK");
        }
    }
}

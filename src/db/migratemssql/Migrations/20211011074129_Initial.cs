using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace migratemssql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EXRPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TEK = table.Column<byte[]>(type: "varbinary(16)", maxLength: 16, nullable: true),
                    RPI = table.Column<byte[]>(type: "varbinary(16)", maxLength: 16, nullable: true),
                    RollingStartIntervalNumber = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RpiDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXRPI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RPI",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<byte[]>(type: "varbinary(16)", maxLength: 16, nullable: true),
                    Metadata = table.Column<byte[]>(type: "varbinary(4)", maxLength: 4, nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RssiMin = table.Column<short>(type: "smallint", nullable: false),
                    RssiMax = table.Column<short>(type: "smallint", nullable: false),
                    MAC = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RPI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TEK",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<byte[]>(type: "varbinary(16)", maxLength: 16, nullable: true),
                    RollingStartIntervalNumber = table.Column<int>(type: "int", nullable: false),
                    TransmissionRiskLevel = table.Column<int>(type: "int", nullable: false),
                    RollingPeriod = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
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

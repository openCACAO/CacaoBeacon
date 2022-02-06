﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenCacao.CacaoBeacon;

namespace migratemssql.Migrations
{
    [DbContext(typeof(CBContextSQLServer))]
    [Migration("20211011074129_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("OpenCacao.CacaoBeacon.ExportRotatingProximityIdentifier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("RPI")
                        .HasMaxLength(16)
                        .HasColumnType("varbinary(16)");

                    b.Property<int>("RollingStartIntervalNumber")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("RpiDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("TEK")
                        .HasMaxLength(16)
                        .HasColumnType("varbinary(16)");

                    b.HasKey("Id");

                    b.ToTable("EXRPI");
                });

            modelBuilder.Entity("OpenCacao.CacaoBeacon.RotatingProximityIdentifier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("EndTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<byte[]>("Key")
                        .HasMaxLength(16)
                        .HasColumnType("varbinary(16)");

                    b.Property<decimal>("MAC")
                        .HasColumnType("decimal(20,0)");

                    b.Property<byte[]>("Metadata")
                        .HasMaxLength(4)
                        .HasColumnType("varbinary(4)");

                    b.Property<short>("RssiMax")
                        .HasColumnType("smallint");

                    b.Property<short>("RssiMin")
                        .HasColumnType("smallint");

                    b.Property<DateTimeOffset>("StartTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("RPI");
                });

            modelBuilder.Entity("OpenCacao.CacaoBeacon.TemporaryExposureKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("Key")
                        .HasMaxLength(16)
                        .HasColumnType("varbinary(16)");

                    b.Property<int>("RollingPeriod")
                        .HasColumnType("int");

                    b.Property<int>("RollingStartIntervalNumber")
                        .HasColumnType("int");

                    b.Property<int>("TransmissionRiskLevel")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("TEK");
                });
#pragma warning restore 612, 618
        }
    }
}
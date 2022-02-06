using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCacao.CacaoBeacon
{
    public class CBContextSQLServer : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(local);Initial Catalog=cacaodb;Integrated Security=True");
        }
        public DbSet<TemporaryExposureKey> TEK => Set<TemporaryExposureKey>();
        public DbSet<RotatingProximityIdentifier> RPI => Set<RotatingProximityIdentifier>();
        public DbSet<ExportRotatingProximityIdentifier> EXRPI => Set<ExportRotatingProximityIdentifier>();
    }
}

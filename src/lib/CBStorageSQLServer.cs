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
        public DbSet<TEK> TEK => Set<TEK>();
        public DbSet<RPI> RPI => Set<RPI>();
        public DbSet<EXRPI> EXRPI => Set<EXRPI>();
    }
}

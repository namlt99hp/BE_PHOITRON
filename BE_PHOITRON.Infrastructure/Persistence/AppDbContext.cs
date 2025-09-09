using BE_PHOITRON.DataEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options) { }

        public DbSet<CongThucPhoi> CongThucPhoi { get; set; } = default!;
        public DbSet<Quang> Quang { get; set; } = default!;
        public DbSet<CongThucPhoi_Quang> CongThucPhoi_Quang { get; set; } = default!;
        public DbSet<CongThucPhoi_TPHH> CongThucPhoi_TPHH { get; set; } = default!;
        public DbSet<Quang_TPHH> Quang_TPHH { get; set; } = default!;
        public DbSet<TP_HoaHoc> TP_HoaHoc { get; set; } = default!;
    }
}

using BE_PHOITRON.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Infrastructure.Persistence
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
           : base(options) { }

        // Core entities
        public DbSet<Tbl_TaiKhoan> Tbl_TaiKhoan { get; set; } = default!;
        public DbSet<Tbl_PhongBan> Tbl_PhongBan { get; set; } = default!;
    }
}

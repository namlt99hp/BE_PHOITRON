using BE_PHOITRON.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options) { }

        // Core entities
        public DbSet<Quang> Quang { get; set; } = default!;
        public DbSet<TP_HoaHoc> TP_HoaHoc { get; set; } = default!;
        
        // Analysis and pricing
        public DbSet<Quang_TP_PhanTich> Quang_TP_PhanTich { get; set; } = default!;
        public DbSet<Quang_Gia_LichSu> Quang_Gia_LichSu { get; set; } = default!;
        
        // Recipe management
        public DbSet<Cong_Thuc_Phoi> Cong_Thuc_Phoi { get; set; } = default!;
        public DbSet<CTP_ChiTiet_Quang> CTP_ChiTiet_Quang { get; set; } = default!;
        public DbSet<CTP_ChiTiet_Quang_TPHH> CTP_ChiTiet_Quang_TPHH { get; set; } = default!;
        public DbSet<CTP_RangBuoc_TPHH> CTP_RangBuoc_TPHH { get; set; } = default!;
        
        // Scenario planning
        public DbSet<Phuong_An_Phoi> Phuong_An_Phoi { get; set; } = default!;
        public DbSet<PA_LuaChon_CongThuc> PA_LuaChon_CongThuc { get; set; } = default!;
        
        // Snapshots and results
        public DbSet<PA_Snapshot_TPHH> PA_Snapshot_TPHH { get; set; } = default!;
        public DbSet<PA_Snapshot_Gia> PA_Snapshot_Gia { get; set; } = default!;
        public DbSet<PA_KetQua_TongHop> PA_KetQua_TongHop { get; set; } = default!;

        // Blast furnace (Lò cao) process parameters
        public DbSet<LoCao_ProcessParam> LoCao_ProcessParam { get; set; } = default!;
        public DbSet<PA_ProcessParamValue> PA_ProcessParamValue { get; set; } = default!;
        
        // Plan result ore mapping
        public DbSet<PA_Quang_KQ> PA_Quang_KQ { get; set; } = default!;
        
        // Statistics system
        public DbSet<ThongKe_Function> ThongKe_Function { get; set; } = default!;
        public DbSet<PA_ThongKe_Result> PA_ThongKe_Result { get; set; } = default!;

        // public DbSet<User> Users => Set<User>();
        // public DbSet<Department> Departments => Set<Department>();
        // public DbSet<Role> Roles => Set<Role>();
        // public DbSet<Permission> Permissions => Set<Permission>();
        // public DbSet<UserRoleMap> UserRoleMaps => Set<UserRoleMap>();
        // public DbSet<RolePermissionMap> RolePermissionMaps => Set<RolePermissionMap>();
        // public DbSet<UserPermissionMap> UserPermissionMaps => Set<UserPermissionMap>();
        
    }
}

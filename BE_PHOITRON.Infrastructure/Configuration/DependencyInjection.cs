using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.Services;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Infrastructure.Persistence;
using BE_PHOITRON.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BE_PHOITRON.Infrastructure.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // Database Context
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("Default")));

            // Base Repository and Unit of Work
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Core Entity Repositories
            services.AddScoped<IQuangRepository, QuangRepository>();
            // Gang module removed
            services.AddScoped<ITP_HoaHocRepository, TP_HoaHocRepository>();
            
            // Analysis and Pricing Repositories
            services.AddScoped<IQuang_TP_PhanTichRepository, Quang_TP_PhanTichRepository>();
            services.AddScoped<IQuang_Gia_LichSuRepository, Quang_Gia_LichSuRepository>();
            
            // Recipe Management Repositories
            services.AddScoped<ICong_Thuc_PhoiRepository, Cong_Thuc_PhoiRepository>();
            services.AddScoped<ICTP_ChiTiet_QuangRepository, CTP_ChiTiet_QuangRepository>();
            services.AddScoped<ICTP_RangBuoc_TPHHRepository, CTP_RangBuoc_TPHHRepository>();
            
            // Scenario Planning Repositories
            services.AddScoped<IPhuong_An_PhoiRepository, Phuong_An_PhoiRepository>();
            services.AddScoped<IPA_LuaChon_CongThucRepository, PA_LuaChon_CongThucRepository>();

            // LoCao Process Params
            services.AddScoped<ILoCaoProcessParamRepository, LoCaoProcessParamRepository>();
            
            // Statistics System
            services.AddScoped<IThongKeRepository, ThongKeRepository>();

            // Application Services
            services.AddScoped<IQuangService, QuangService>();
            // Gang module removed
            services.AddScoped<ITP_HoaHocService, TP_HoaHocService>();
            services.AddScoped<IQuang_TP_PhanTichService, Quang_TP_PhanTichService>();
            services.AddScoped<IQuang_Gia_LichSuService, Quang_Gia_LichSuService>();
            services.AddScoped<ICong_Thuc_PhoiService, Cong_Thuc_PhoiService>();
            services.AddScoped<ICTP_ChiTiet_QuangService, CTP_ChiTiet_QuangService>();
            services.AddScoped<ICTP_RangBuoc_TPHHService, CTP_RangBuoc_TPHHService>();
            services.AddScoped<IPlanningService, PlanningService>();
            services.AddScoped<IPhuong_An_PhoiService, Phuong_An_PhoiService>();
            services.AddScoped<IPA_LuaChon_CongThucService, PA_LuaChon_CongThucService>();
            services.AddScoped<ILoCaoProcessParamService, LoCaoProcessParamService>();
            services.AddScoped<IThongKeService, ThongKeService>();

            return services;
        }
    }
}

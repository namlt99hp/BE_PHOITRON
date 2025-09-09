using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.Services;                      // QuangService
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Infrastructure.Persistence;                 // AppDbContext
using BE_PHOITRON.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("Default")));

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // add repo
            services.AddScoped<IQuangRepository, QuangRepository>();
            services.AddScoped<ITPHHRepository, TPHHRepository>();
            services.AddScoped<ICongThucPhoiRepository, CongThucPhoiRepository>();

            // Application services ở Api hoặc thêm extension riêng trong Application:
            services.AddScoped<IQuangService, QuangService>();
            services.AddScoped<ITPHHService, TPHHService>();
            services.AddScoped<ICongThucPhoiService, CongThucPhoiService>();

            return services;
        }
    }
}

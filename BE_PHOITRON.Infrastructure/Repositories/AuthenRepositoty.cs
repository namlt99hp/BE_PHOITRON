using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using BE_PHOITRON.Infrastructure.Shared;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class AuthenRepository : IAuthenRepository
    {
        private readonly AuthDbContext _authDb;

        public AuthenRepository(AuthDbContext authDb) { _authDb = authDb; }

        public async Task<LoginResponse?> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            // 🔒 Mã hóa mật khẩu bằng MD5
            string hashedPassword = SecurityHelper.ToMD5(loginDto.password);

            var user = await _authDb.Tbl_TaiKhoan
                .FirstOrDefaultAsync(x =>
                    x.TenTaiKhoan == loginDto.username &&
                    x.MatKhau == hashedPassword, ct);
            if (user == null)
                return null;

            var phongBan = await _authDb.Tbl_PhongBan
                .FirstOrDefaultAsync(x => x.ID_PhongBan == user.ID_PhongBan, ct);
            if (phongBan == null)
                return null;

            var result = new LoginResponse(
                user.ID_TaiKhoan,
                user.TenTaiKhoan ?? string.Empty,
                user.HoVaTen ?? string.Empty,
                user.ChuKy ?? string.Empty,
                user.PhongBan_API,
                phongBan.TenPhongBan,
                phongBan.TenNgan,
                user.ID_PhongBan,
                user.ID_PhanXuong,
                user.Xuong_API
            );
            return result;
        }
    }
}

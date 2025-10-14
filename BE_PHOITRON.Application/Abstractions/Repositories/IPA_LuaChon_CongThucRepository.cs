using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IPA_LuaChon_CongThucRepository : IRepository<PA_LuaChon_CongThuc>
    {
        Task<IReadOnlyList<PA_LuaChon_CongThuc>> GetByPhuongAnAsync(int idPhuongAn, CancellationToken ct = default);
        Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<PA_LuaChon_CongThuc> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);

        Task<(int total, IReadOnlyList<PA_LuaChon_CongThuc> data)> SearchPagedAdvancedAsync(
            int page,
            int pageSize,
            int? idPhuongAn = null,
            int? idQuangDauRa = null,
            int? idCongThucPhoi = null,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            CancellationToken ct = default);
    }
}

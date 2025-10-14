using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ICTP_ChiTiet_QuangRepository : IRepository<CTP_ChiTiet_Quang>
    {
        Task<IReadOnlyList<CTP_ChiTiet_Quang>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default);
        Task<bool> ValidateTotalPercentageAsync(int idCongThucPhoi, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<CTP_ChiTiet_Quang> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
    }
}

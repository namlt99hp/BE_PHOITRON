using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ICTP_RangBuoc_TPHHRepository : IRepository<CTP_RangBuoc_TPHH>
    {
        Task<IReadOnlyList<CTP_RangBuoc_TPHH>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<CTP_RangBuoc_TPHH> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
    }
}

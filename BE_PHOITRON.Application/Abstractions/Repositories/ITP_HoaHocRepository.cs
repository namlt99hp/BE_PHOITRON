using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ITP_HoaHocRepository : IRepository<TP_HoaHoc>
    {
        Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default);
        Task<IReadOnlyList<TP_HoaHoc>> GetActiveAsync(CancellationToken ct = default);
        Task<(int total, IReadOnlyList<TP_HoaHoc> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}


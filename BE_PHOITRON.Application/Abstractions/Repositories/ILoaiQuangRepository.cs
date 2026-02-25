using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ILoaiQuangRepository : IRepository<LoaiQuang>
    {
        Task<(int total, IReadOnlyList<LoaiQuang> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);

        Task<IReadOnlyList<LoaiQuang>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string maLoaiQuang, CancellationToken ct = default);
    }
}


using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ILoQuangRepository : IRepository<LoQuang>
    {
        Task<(int total, IReadOnlyList<LoQuang> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);

        Task<IReadOnlyList<LoQuang>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string maLoQuang, CancellationToken ct = default);
    }
}


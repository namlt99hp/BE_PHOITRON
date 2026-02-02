using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ICong_Thuc_PhoiRepository : IRepository<Cong_Thuc_Phoi>
    {
        Task<bool> ExistsByCodeAsync(string maCongThuc, CancellationToken ct = default);
        Task<Cong_Thuc_Phoi?> GetByQuangDauRaAsync(int idQuangDauRa, CancellationToken ct = default);
        Task<IReadOnlyList<Cong_Thuc_Phoi>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> HasOverlappingPeriodAsync(int idQuangDauRa, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<Cong_Thuc_Phoi> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        Task<bool> DeleteCongThucPhoiAsync(int id, CancellationToken ct = default);
        Task<bool> DeleteCongThucPhoiWithRelatedDataAsync(int id, CancellationToken ct = default);
    }
}

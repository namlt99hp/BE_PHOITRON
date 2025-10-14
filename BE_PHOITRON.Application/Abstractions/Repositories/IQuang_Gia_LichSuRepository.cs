using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IQuang_Gia_LichSuRepository : IRepository<Quang_Gia_LichSu>
    {
        Task<IReadOnlyList<Quang_Gia_LichSu>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default);
        Task<IReadOnlyList<Quang_Gia_LichSu>> GetByQuangAsync(int idQuang, CancellationToken ct = default);
        Task<Quang_Gia_LichSu?> GetCurrentPriceAsync(int idQuang, DateTimeOffset ngayTinhToan, CancellationToken ct = default);
        Task<bool> HasOverlappingPeriodAsync(int idQuang, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<Quang_Gia_LichSu> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
    }
}

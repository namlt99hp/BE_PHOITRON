using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IQuang_TP_PhanTichRepository : IRepository<Quang_TP_PhanTich>
    {
        Task<IReadOnlyList<Quang_TP_PhanTich>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default);
        Task<IReadOnlyList<Quang_TP_PhanTich>> GetByQuangAsync(int idQuang, CancellationToken ct = default);
        Task<bool> HasOverlappingPeriodAsync(int idQuang, int idTPHH, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<Quang_TP_PhanTich> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        // Formula calculation operations
        Task<Dictionary<int, decimal>> CalculateTPHHFormulasAsync(int quangId, CancellationToken ct = default);
    }
}

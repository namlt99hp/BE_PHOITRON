using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IQuang_TP_PhanTichService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<Quang_TP_PhanTichResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<Quang_TP_PhanTichResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Quang_TP_PhanTichCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(Quang_TP_PhanTichUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(Quang_TP_PhanTichUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<IReadOnlyList<Quang_TP_PhanTichResponse>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default);
        Task<IReadOnlyList<Quang_TP_PhanTichResponse>> GetByQuangAsync(int idQuang, CancellationToken ct = default);
        Task<bool> HasOverlappingPeriodAsync(int idQuang, int idTPHH, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default);
        
        // Formula calculation operations
        Task<Dictionary<int, decimal>> CalculateTPHHFormulasAsync(int quangId, CancellationToken ct = default);
    }
}

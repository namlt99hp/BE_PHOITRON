using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IQuang_Gia_LichSuService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<Quang_Gia_LichSuResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<Quang_Gia_LichSuResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Quang_Gia_LichSuCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(Quang_Gia_LichSuUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(Quang_Gia_LichSuUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<IReadOnlyList<Quang_Gia_LichSuResponse>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default);
        Task<IReadOnlyList<Quang_Gia_LichSuResponse>> GetByQuangAsync(int idQuang, CancellationToken ct = default);
        Task<bool> HasOverlappingPeriodAsync(int idQuang, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default);
        Task<decimal?> GetGiaByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default);
    }
}

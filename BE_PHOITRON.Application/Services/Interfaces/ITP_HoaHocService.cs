using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ITP_HoaHocService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<TP_HoaHocResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<TP_HoaHocResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(TP_HoaHocCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(TP_HoaHocUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(TP_HoaHocUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default);
        Task<IReadOnlyList<TP_HoaHocResponse>> GetActiveAsync(CancellationToken ct = default);
    }
}

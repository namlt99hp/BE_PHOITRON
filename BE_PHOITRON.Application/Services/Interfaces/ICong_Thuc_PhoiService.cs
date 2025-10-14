using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ICong_Thuc_PhoiService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<Cong_Thuc_PhoiResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<Cong_Thuc_PhoiResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Cong_Thuc_PhoiCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(Cong_Thuc_PhoiUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(Cong_Thuc_PhoiUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<bool> ExistsByCodeAsync(string maCongThuc, CancellationToken ct = default);
        Task<IReadOnlyList<Cong_Thuc_PhoiResponse>> GetByQuangDauRaAsync(int idQuangDauRa, CancellationToken ct = default);
        Task<IReadOnlyList<Cong_Thuc_PhoiResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> HasOverlappingPeriodAsync(int idQuangDauRa, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default);
        Task<bool> ValidateTotalPercentageAsync(int idCongThucPhoi, CancellationToken ct = default);
    }
}

using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ICTP_ChiTiet_QuangService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<CTP_ChiTiet_QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<CTP_ChiTiet_QuangResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CTP_ChiTiet_QuangCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(CTP_ChiTiet_QuangUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(CTP_ChiTiet_QuangUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<IReadOnlyList<CTP_ChiTiet_QuangResponse>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default);
        Task<bool> ValidateTotalPercentageAsync(int idCongThucPhoi, CancellationToken ct = default);
    }
}

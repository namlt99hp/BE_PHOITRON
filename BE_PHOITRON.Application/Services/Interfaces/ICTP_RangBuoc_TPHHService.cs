using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ICTP_RangBuoc_TPHHService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<CTP_RangBuoc_TPHHResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<CTP_RangBuoc_TPHHResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CTP_RangBuoc_TPHHCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(CTP_RangBuoc_TPHHUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(CTP_RangBuoc_TPHHUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<IReadOnlyList<CTP_RangBuoc_TPHHResponse>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default);
    }
}

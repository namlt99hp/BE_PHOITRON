using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IPA_LuaChon_CongThucService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<PA_LuaChon_CongThucResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<PA_LuaChon_CongThucResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(PA_LuaChon_CongThucCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(PA_LuaChon_CongThucUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(PA_LuaChon_CongThucUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<IReadOnlyList<PA_LuaChon_CongThucResponse>> GetByPhuongAnAsync(int idPhuongAn, CancellationToken ct = default);
        Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default);

        Task<(int total, IReadOnlyList<PA_LuaChon_CongThucResponse> data)> SearchPagedAdvancedAsync(
            int page,
            int pageSize,
            int? idPhuongAn = null,
            int? idQuangDauRa = null,
            int? idCongThucPhoi = null,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            CancellationToken ct = default);
    }
}

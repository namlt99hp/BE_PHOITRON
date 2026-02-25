using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ILoaiQuangService
    {
        Task<(int total, IReadOnlyList<LoaiQuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);

        Task<LoaiQuangResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> UpsertAsync(LoaiQuangUpsertDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<LoaiQuangResponse>> GetActiveAsync(CancellationToken ct = default);
    }
}


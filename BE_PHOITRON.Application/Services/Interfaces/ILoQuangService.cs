using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ILoQuangService
    {
        Task<(int total, IReadOnlyList<LoQuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);

        Task<LoQuangResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> UpsertAsync(LoQuangUpsertDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<LoQuangResponse>> GetActiveAsync(CancellationToken ct = default);
    }
}


using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ILoCaoProcessParamService
    {
        Task<IReadOnlyList<LoCao_ProcessParam>> GetAllAsync(CancellationToken ct = default);
        Task<LoCao_ProcessParam?> GetByIdAsync(int id, CancellationToken ct = default);
      Task<object?> GetDetailByIdAsync(int id, CancellationToken ct = default);
        Task<LoCao_ProcessParam> CreateAsync(LoCao_ProcessParam entity, CancellationToken ct = default);
        Task UpdateAsync(int id, LoCao_ProcessParam payload, CancellationToken ct = default);
        Task<LoCao_ProcessParam> UpsertAsync(LoCao_ProcessParam entity, CancellationToken ct = default);
        Task SoftDeleteAsync(int id, CancellationToken ct = default);
        Task LinkOreAsync(int id, int? oreId, CancellationToken ct = default);
        Task<(IReadOnlyList<LoCao_ProcessParam> Items, int Total)> SearchPagedAsync(int page, int size, string? sortBy, string? sortDir, string? search, CancellationToken ct = default);
        Task<IReadOnlyList<BE_PHOITRON.Application.ResponsesModels.ProcessParamConfiguredResponse>> GetConfiguredByPaIdAsync(int paLuaChonCongThucId, CancellationToken ct = default);
        Task ConfigureProcessParamsForPlanAsync(int paLuaChonCongThucId, List<int> processParamIds, List<int> thuTuParams, CancellationToken ct = default);

        // New: Upsert values for plan
        Task UpsertValuesForPlanAsync(int paLuaChonCongThucId, IReadOnlyList<(int IdProcessParam, decimal GiaTri, int? ThuTuParam)> items, CancellationToken ct = default);

        // Removed: use GetConfiguredByPaIdAsync to include configured values
    }
}



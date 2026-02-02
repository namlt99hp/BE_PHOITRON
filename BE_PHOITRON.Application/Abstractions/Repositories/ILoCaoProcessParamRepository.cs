using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ILoCaoProcessParamRepository
    {
        Task<IReadOnlyList<LoCao_ProcessParam>> GetAllAsync(CancellationToken ct = default);
        Task<LoCao_ProcessParam?> GetByIdAsync(int id, CancellationToken ct = default);
      Task<object?> GetLinkedOreBasicAsync(int oreId, CancellationToken ct = default);
        Task<LoCao_ProcessParam> AddAsync(LoCao_ProcessParam entity, CancellationToken ct = default);
        Task UpdateAsync(LoCao_ProcessParam entity, CancellationToken ct = default);
        Task<LoCao_ProcessParam> UpsertAsync(LoCao_ProcessParam entity, CancellationToken ct = default);
        Task SoftDeleteAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task LinkOreAsync(int id, int? oreId, CancellationToken ct = default);

        Task<(IReadOnlyList<LoCao_ProcessParam> Items, int Total)> SearchPagedAsync(
            int page, int size, string? sortBy, string? sortDir, string? search, CancellationToken ct = default);
        Task<IReadOnlyList<Application.ResponsesModels.ProcessParamConfiguredResponse>> GetConfiguredByPaIdAsync(int paLuaChonCongThucId, CancellationToken ct = default);
        Task ConfigureProcessParamsForPlanAsync(int paLuaChonCongThucId, List<int> processParamIds, List<int> thuTuParams, CancellationToken ct = default);

        // New: Upsert parameter values for a plan (PA_ProcessParamValue.GiaTri)
        Task UpsertValuesForPlanAsync(int paLuaChonCongThucId, IReadOnlyList<(int IdProcessParam, decimal GiaTri, int? ThuTuParam)> items, CancellationToken ct = default);

        // Removed GetValuesByPaIdAsync: use GetConfiguredByPaIdAsync to include configured values
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class LoCaoProcessParamService : ILoCaoProcessParamService
    {
        private readonly ILoCaoProcessParamRepository _repo;

        public LoCaoProcessParamService(ILoCaoProcessParamRepository repo)
        {
            _repo = repo;
        }

        public Task<IReadOnlyList<LoCao_ProcessParam>> GetAllAsync(CancellationToken ct = default)
            => _repo.GetAllAsync(ct);

        public Task<LoCao_ProcessParam?> GetByIdAsync(int id, CancellationToken ct = default)
            => _repo.GetByIdAsync(id, ct);

      public async Task<object?> GetDetailByIdAsync(int id, CancellationToken ct = default)
      {
          var item = await _repo.GetByIdAsync(id, ct);
          if (item == null) return null;
          return new
          {
              id = item.ID,
              code = item.Code,
              ten = item.Ten,
              donVi = item.DonVi,
              id_Quang_LienKet = item.ID_Quang_LienKet,
              scope = item.Scope,
              thuTu = item.ThuTu,
              isCalculated = item.IsCalculated,
              calcFormula = item.CalcFormula,
              linkedOre = item.ID_Quang_LienKet == null ? null : await _repo.GetLinkedOreBasicAsync(item.ID_Quang_LienKet.Value, ct)
          };
      }

        public Task<LoCao_ProcessParam> CreateAsync(LoCao_ProcessParam entity, CancellationToken ct = default)
            => _repo.AddAsync(entity, ct);

        public async Task UpdateAsync(int id, LoCao_ProcessParam payload, CancellationToken ct = default)
        {
            var exist = await _repo.GetByIdAsync(id, ct);
            if (exist == null) return; // or throw

            exist.Code = payload.Code;
            exist.Ten = payload.Ten;
            exist.DonVi = payload.DonVi;
            exist.ID_Quang_LienKet = payload.ID_Quang_LienKet;
            exist.Scope = payload.Scope;
            exist.ThuTu = payload.ThuTu;
            exist.IsCalculated = payload.IsCalculated;
            exist.CalcFormula = payload.CalcFormula;

            await _repo.UpdateAsync(exist, ct);
        }

        public Task SoftDeleteAsync(int id, CancellationToken ct = default)
            => _repo.SoftDeleteAsync(id, ct);

        public Task DeleteAsync(int id, CancellationToken ct = default)
            => _repo.DeleteAsync(id, ct);

        public Task LinkOreAsync(int id, int? oreId, CancellationToken ct = default)
            => _repo.LinkOreAsync(id, oreId, ct);

        public async Task<LoCao_ProcessParam> UpsertAsync(LoCao_ProcessParam entity, CancellationToken ct = default)
        {
            return await _repo.UpsertAsync(entity, ct);
        }

        public Task<(IReadOnlyList<LoCao_ProcessParam> Items, int Total)> SearchPagedAsync(int page, int size, string? sortBy, string? sortDir, string? search, CancellationToken ct = default)
            => _repo.SearchPagedAsync(page, size, sortBy, sortDir, search, ct);

        public Task<IReadOnlyList<BE_PHOITRON.Application.ResponsesModels.ProcessParamConfiguredResponse>> GetConfiguredByPaIdAsync(int paLuaChonCongThucId, CancellationToken ct = default)
            => _repo.GetConfiguredByPaIdAsync(paLuaChonCongThucId, ct);

        public Task ConfigureProcessParamsForPlanAsync(int paLuaChonCongThucId, List<int> processParamIds, List<int> thuTuParams, CancellationToken ct = default)
            => _repo.ConfigureProcessParamsForPlanAsync(paLuaChonCongThucId, processParamIds, thuTuParams, ct);

        public Task UpsertValuesForPlanAsync(int paLuaChonCongThucId, IReadOnlyList<(int IdProcessParam, decimal GiaTri, int? ThuTuParam)> items, CancellationToken ct = default)
            => _repo.UpsertValuesForPlanAsync(paLuaChonCongThucId, items, ct);

        // Removed: use GetConfiguredByPaIdAsync which now includes configured values
    }
}



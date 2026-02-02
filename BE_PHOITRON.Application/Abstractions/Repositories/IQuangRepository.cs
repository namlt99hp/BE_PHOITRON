using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IQuangRepository : IRepository<Quang>
    {
        Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default);
        Task<bool> ExistsByCodeOrNameAsync(string maQuang, string? tenQuang, int? excludeId = null, CancellationToken ct = default);
        Task<IReadOnlyList<Quang>> GetByLoaiAsync(int loaiQuang, CancellationToken ct = default);
        Task<IReadOnlyList<Quang>> GetActiveAsync(CancellationToken ct = default);

        Task<(int total, IReadOnlyList<QuangResponse> data)> SearchPagedAsync(
            int page,
            int pageSize,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            int[]? loaiQuang = null,
            bool? isGangTarget = null,
            CancellationToken ct = default);

        Task<QuangDetailResponse?> GetDetailByIdAsync(int id, CancellationToken ct = default);
        Task<QuangDetailResponse?> GetLatestGangTargetAsync(CancellationToken ct = default);
        Task<GangTemplateConfigResponse?> GetGangTemplateConfigAsync(int? gangId = null, CancellationToken ct = default);
        Task<GangDichConfigDetailResponse?> GetGangDichDetailWithConfigAsync(int gangId, CancellationToken ct = default);

        Task<int> UpsertWithThanhPhanAsync(QuangUpsertWithThanhPhanDto dto, CancellationToken ct = default);
        Task<int> UpsertKetQuaWithThanhPhanAsync(QuangKetQuaUpsertDto dto, CancellationToken ct = default);
        Task<int> UpsertGangDichWithConfigAsync(GangDichConfigUpsertDto dto, CancellationToken ct = default);

        Task<IReadOnlyList<OreChemistryBatchItem>> GetOreChemistryBatchAsync(IReadOnlyList<int> quangIds, CancellationToken ct = default);
        Task<IReadOnlyList<FormulaByOutputOreResponse>> GetFormulasByOutputOreIdsAsync(IReadOnlyList<int> outputOreIds, CancellationToken ct = default);

        Task<int?> GetSlagIdByGangIdAsync(int gangId, CancellationToken ct = default);
        Task<(QuangDetailResponse? gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryByPlanAsync(int planId, CancellationToken ct = default);

        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> DeleteQuangWithRelatedDataAsync(int id, ICong_Thuc_PhoiRepository congThucPhoiRepo, CancellationToken ct = default);
        Task<bool> DeleteGangDichWithRelatedDataAsync(int gangDichId, IPhuong_An_PhoiRepository phuongAnRepo, ICong_Thuc_PhoiRepository congThucRepo, CancellationToken ct = default);
    }
}
using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IPhuong_An_PhoiRepository : IRepository<Phuong_An_Phoi>
    {
        Task<IReadOnlyList<Phuong_An_Phoi>> GetByQuangDichAsync(int idQuangDich, CancellationToken ct = default);
        Task<IReadOnlyList<Phuong_An_Phoi>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default);
        Task<(int total, IReadOnlyList<Phuong_An_Phoi> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);

        // Mix: create output ore + save formula + details + constraints, and link to plan
        Task<int> MixAsync(MixQuangRequestDto dto, CancellationToken ct = default);
        Task<int> MixWithCompleteDataAsync(MixWithCompleteDataDto dto, CancellationToken ct = default);
        Task<CongThucPhoiDetailResponse?> GetCongThucPhoiDetailAsync(int congThucPhoiId, CancellationToken ct = default);
        Task<PhuongAnWithFormulasResponse?> GetFormulasByPlanAsync(int idPhuongAn, CancellationToken ct = default);
        Task<PhuongAnWithMilestonesResponse?> GetFormulasByPlanWithDetailsAsync(int idPhuongAn, CancellationToken ct = default);
        Task<CongThucPhoiDetailMinimal?> GetDetailMinimalAsync(int congThucPhoiId, CancellationToken ct = default);

        // Clone operations
        Task<int> ClonePlanAsync(ClonePlanRequestDto dto, CancellationToken ct = default);
        Task<int> CloneMilestonesAsync(CloneMilestonesRequestDto dto, CancellationToken ct = default);
        Task<int> CloneQuangKetQuaAsync(int idQuangNguon, int idPhuongAn, int? idGangDich = null, int? nguoiTao = null, CancellationToken ct = default);
        Task<int> UpsertPhuongAnPhoiAsync(Phuong_An_PhoiUpsertDto dto, CancellationToken ct = default);
        
        // Delete operations
        Task<bool> DeletePlanAsync(int id, ICong_Thuc_PhoiRepository congThucRepo, IQuangRepository quangRepo, CancellationToken ct = default);
        Task<bool> DeletePlanWithRelatedDataAsync(int planId, ICong_Thuc_PhoiRepository congThucRepo, IQuangRepository quangRepo, CancellationToken ct = default);

        // Section data retrieval
        Task<List<PlanSectionDto>> GetPlanSectionsByGangDichAsync(int gangDichId, bool includeThieuKet = true, bool includeLoCao = true, CancellationToken ct = default);
        
        // Clone gang with all plans
        Task<int> CloneGangWithAllPlansAsync(int sourceGangId, int newGangId, ClonePlanRequestDto baseOptions, CancellationToken ct = default);
    }
}

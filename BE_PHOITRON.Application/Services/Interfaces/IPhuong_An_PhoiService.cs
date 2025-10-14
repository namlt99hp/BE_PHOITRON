using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IPhuong_An_PhoiService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<Phuong_An_PhoiResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default);
        
        Task<Phuong_An_PhoiResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(Phuong_An_PhoiCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(Phuong_An_PhoiUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(Phuong_An_PhoiUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> DeletePlanWithRelatedDataAsync(int planId, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<IReadOnlyList<Phuong_An_PhoiResponse>> GetByQuangDichAsync(int idQuangDich, CancellationToken ct = default);
        Task<IReadOnlyList<Phuong_An_PhoiResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default);
        
        // Complex Business Logic
        Task<PhuongAnTinhToanResponse?> TinhToanPhuongAnAsync(int idPhuongAn, CancellationToken ct = default);
        Task<SoSanhPhuongAnResponse> SoSanhPhuongAnAsync(List<int> idPhuongAn, CancellationToken ct = default);

        // Mix (create output ore + link to plan)
        Task<int> MixAsync(MixQuangRequestDto dto, CancellationToken ct = default);
        Task<CongThucPhoiDetailResponse?> GetCongThucPhoiDetailAsync(int congThucPhoiId, CancellationToken ct = default);
        Task<PhuongAnWithFormulasResponse?> GetFormulasByPlanAsync(int idPhuongAn, CancellationToken ct = default);
        Task<PhuongAnWithMilestonesResponse?> GetFormulasByPlanWithDetailsAsync(int idPhuongAn, CancellationToken ct = default);
        Task<CongThucPhoiDetailMinimal?> GetDetailMinimalAsync(int congThucPhoiId, CancellationToken ct = default);

        // Clone operations
        Task<int> ClonePlanAsync(ClonePlanRequestDto dto, CancellationToken ct = default);
        Task<int> CloneMilestonesAsync(CloneMilestonesRequestDto dto, CancellationToken ct = default);
    }
}

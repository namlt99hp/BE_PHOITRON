using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IQuangService
    {
        // CRUD Operations
        Task<(int total, IReadOnlyList<QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, int[]? loaiQuang = null, bool? isGangTarget = null, CancellationToken ct = default);
        
        Task<QuangResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<QuangDetailResponse?> GetDetailByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(QuangCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(QuangUpdateDto dto, CancellationToken ct = default);
        Task<int> UpsertAsync(QuangUpsertDto dto, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        
        // Business Logic Operations
        Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default);
        Task<IReadOnlyList<QuangResponse>> GetByLoaiAsync(int loaiQuang, CancellationToken ct = default);
        Task<IReadOnlyList<QuangResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
        

        // Upsert quặng gang đích (loại 2 - Gang)
        Task<int> UpsertWithThanhPhanAsync(QuangUpsertWithThanhPhanDto dto, CancellationToken ct = default);
        
        // Upsert Gang/Xỉ result ores with plan mapping
        Task<int> UpsertKetQuaWithThanhPhanAsync(QuangKetQuaUpsertDto dto, CancellationToken ct = default);
        
        Task<IReadOnlyList<OreChemistryBatchItem>> GetOreChemistryBatchAsync(IReadOnlyList<int> quangIds, CancellationToken ct = default);
        Task<IReadOnlyList<FormulaByOutputOreResponse>> GetFormulasByOutputOreIdsAsync(IReadOnlyList<int> outputOreIds, CancellationToken ct = default);
        Task<int?> GetSlagIdByGangIdAsync(int gangId, CancellationToken ct = default);
        Task<(QuangDetailResponse gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryAsync(int gangId, CancellationToken ct = default);
        Task<(QuangDetailResponse? gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryByPlanAsync(int planId, CancellationToken ct = default);
    }
}
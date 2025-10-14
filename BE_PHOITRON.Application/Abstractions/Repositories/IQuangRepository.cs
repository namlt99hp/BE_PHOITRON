using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Domain.Entities;
using System.Linq.Expressions;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IQuangRepository : IRepository<Quang>
    {
        Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default);
        Task<IReadOnlyList<Quang>> GetByLoaiAsync(int loaiQuang, CancellationToken ct = default);
        Task<IReadOnlyList<Quang>> GetActiveAsync(CancellationToken ct = default);
       
        // Search with current price enriched inside repository
        Task<(int total, IReadOnlyList<QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, int? loaiQuang = null, CancellationToken ct = default);
        
        
        // Get quặng detail with pricing and chemical composition
        Task<QuangDetailResponse?> GetDetailByIdAsync(int id, CancellationToken ct = default);
        

        // Upsert quặng gang đích (loại 2 - Gang) chỉ có thông tin cơ bản + thành phần hóa học
        Task<int> UpsertWithThanhPhanAsync(QuangUpsertWithThanhPhanDto dto, CancellationToken ct = default);
        
        // Upsert Gang/Xỉ result ores with plan mapping
        Task<int> UpsertKetQuaWithThanhPhanAsync(QuangKetQuaUpsertDto dto, CancellationToken ct = default);
        
        Task<IReadOnlyList<OreChemistryBatchItem>> GetOreChemistryBatchAsync(IReadOnlyList<int> quangIds, CancellationToken ct = default);
        Task<IReadOnlyList<FormulaByOutputOreResponse>> GetFormulasByOutputOreIdsAsync(IReadOnlyList<int> outputOreIds, CancellationToken ct = default);
        Task<int?> GetSlagIdByGangIdAsync(int gangId, CancellationToken ct = default);
        Task<(QuangDetailResponse? gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryByPlanAsync(int planId, CancellationToken ct = default);
    }
}
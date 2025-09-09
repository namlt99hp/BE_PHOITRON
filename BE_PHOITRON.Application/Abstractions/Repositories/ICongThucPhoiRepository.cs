using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ICongThucPhoiRepository : Base.IRepository<CongThucPhoi>
    {
        Task<(int total, IReadOnlyList<CongThucPhoi> data)> SearchPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default);
        Task<int> UpdateCongThucPTDto(UpdateCongThucPTDto dto, CancellationToken ct = default);
        Task<CongThucPhoiDetailRespone> GetCongThucPhoiDetailAsync(int id, CancellationToken ct = default);
        Task<int> UpsertCongThucPTAsync(UpsertCongThucPTDto dto, CancellationToken ct = default);
        Task<UpsertAndConfirmResult> UpsertAndConfirmAsync(UpsertAndConfirmDto dto, CancellationToken ct = default);
        Task<CongThucEditVm?> GetForEditAsync(int formulaId, CancellationToken ct = default);

        // Optional: danh sách công thức theo 1 neo để FE so sánh
        Task<NeoDashboardVm?> GetByNeoAsync(int quangNeoId, CancellationToken ct = default);
    }
}

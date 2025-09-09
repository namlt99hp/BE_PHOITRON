using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ICongThucPhoiService
    {
        Task<(int total, IReadOnlyList<CongThucPhoiResponse> data)> ListAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default);
        Task<CongThucPhoiResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CreateCongThucPhoiDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, UpdateCongThucPhoiDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<int> UpdateCongThucPTDto(UpdateCongThucPTDto dto, CancellationToken ct = default);
        Task<CongThucPhoiDetailRespone> GetCongThucPhoiDetail(int id, CancellationToken ct = default);
        Task<int> UpsertCongThucPhoiTron(UpsertCongThucPTDto dto, CancellationToken ct = default);

        //----------------------------
        Task<UpsertAndConfirmResult> UpsertAndConfirmAsync(UpsertAndConfirmDto dto, CancellationToken ct = default);
        Task<CongThucEditVm?> GetForEditAsync(int formulaId, CancellationToken ct = default);

        // Optional: danh sách công thức theo 1 neo để FE so sánh
        Task<NeoDashboardVm?> GetByNeoAsync(int quangNeoId, CancellationToken ct = default);
    }
}

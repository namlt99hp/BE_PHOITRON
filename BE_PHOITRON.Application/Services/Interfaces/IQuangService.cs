using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface IQuangService
    {
        Task<(int total, IReadOnlyList<QuangResponse> data)> ListAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default);
        Task<QuangResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(QuangCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, QuangUpdateDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        Task<int> UpdateTPHH(Quang_TPHHUpdateDto dto, CancellationToken ct = default);

        Task<QuangDetailResponse> GetDetailQuang(int id, CancellationToken ct = default);
        Task<int> UpsertAsync(UpsertQuangMuaDto dto, CancellationToken ct = default);

        Task<List<QuangDetailResponse>> getOreChemistryBatch(List<int> id_Quangs, CancellationToken ct = default);
        Task<IReadOnlyList<QuangItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default);
    }
}

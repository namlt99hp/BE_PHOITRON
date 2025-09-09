using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IQuangRepository : Base.IRepository<Quang>
    {
        // cac action them
        Task<(int total, IReadOnlyList<Quang> data)> SearchPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default);
        Task<Quang?> GetWithGiaHienHanhAsync(int id, DateTime? taiThoiDiem, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default);
        Task<int> UpdateTPHH(Quang_TPHHUpdateDto dto, CancellationToken ct = default);

        Task<QuangDetailResponse> GetDetailQuang(int id, CancellationToken ct = default);
        Task<int> UpsertAsync(UpsertQuangMuaDto dto, CancellationToken ct = default);

        Task<List<QuangDetailResponse>> getOreChemistryBatch(List<int> id_Quangs, CancellationToken ct = default);
        Task<IReadOnlyList<QuangItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default);
    }
}

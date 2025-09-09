using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface ITPHHRepository : Base.IRepository<TP_HoaHoc>
    {
        Task<(int total, IReadOnlyList<TP_HoaHoc> data)> SearchPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default);
        Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default);
        Task<IReadOnlyList<TPHHItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default);
    }
}

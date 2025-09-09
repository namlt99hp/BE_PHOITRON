using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Services.Interfaces
{
    public interface ITPHHService
    {
        Task<(int total, IReadOnlyList<TPHHResponse> data)> ListAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default);
        Task<TPHHResponse?> GetAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(TPHHCreateDto dto, CancellationToken ct = default);
        Task<bool> UpdateAsync(int id, TPHHUpdateDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<TPHHItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default);
    }
}

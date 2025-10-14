using BE_PHOITRON.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Abstractions.Repositories
{
    public interface IPAProcessParamValueRepository
    {
        Task<IReadOnlyList<PA_ProcessParamValue>> GetByPhuongAnIdAsync(int phuongAnId, CancellationToken ct = default);
        Task<PA_ProcessParamValue> AddAsync(PA_ProcessParamValue entity, CancellationToken ct = default);
        Task UpdateAsync(int id, PA_ProcessParamValue payload, CancellationToken ct = default);
    }
}




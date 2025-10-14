using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class PAProcessParamValueRepository : IPAProcessParamValueRepository
    {
        private readonly AppDbContext _db;

        public PAProcessParamValueRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<PA_ProcessParamValue>> GetByPhuongAnIdAsync(int phuongAnId, CancellationToken ct = default)
        {
            return await _db.PA_ProcessParamValue
                .Where(x => x.ID_Phuong_An == phuongAnId)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<PA_ProcessParamValue> AddAsync(PA_ProcessParamValue entity, CancellationToken ct = default)
        {
            var entry = await _db.PA_ProcessParamValue.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return entry.Entity;
        }

        public async Task UpdateAsync(int id, PA_ProcessParamValue payload, CancellationToken ct = default)
        {
            var entity = await _db.PA_ProcessParamValue.FindAsync(new object[] { id }, ct);
            if (entity == null) return;

            entity.GiaTri = payload.GiaTri;
            entity.ThuTuParam = payload.ThuTuParam;
            await _db.SaveChangesAsync(ct);
        }
    }
}




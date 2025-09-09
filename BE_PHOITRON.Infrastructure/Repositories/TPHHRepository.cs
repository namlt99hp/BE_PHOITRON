using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.DataEntities;
using BE_PHOITRON.Infrastructure.Persistence;
using BE_PHOITRON.Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class TPHHRepository : BaseRepository<TP_HoaHoc>, ITPHHRepository
    {
        public TPHHRepository(AppDbContext db) : base(db)
        {
        }
        public Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default)
            => _set.AnyAsync(x => x.Ma_TPHH == maTPHH, ct);

        public async Task<(int total, IReadOnlyList<TP_HoaHoc> data)> SearchPagedAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<TP_HoaHoc> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.Ma_TPHH.Contains(search) || (x.Ten_TPHH ?? "").Contains(search) || (x.GhiChu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && CheckValidPropertyPath.IsValidPropertyPath<TP_HoaHoc>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");              
            }
            else
            {
                q = q.OrderByDescending(x => x.NgayTao);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }

        public async Task<IReadOnlyList<TPHHItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default)
        {
            if (IDs is null || IDs.Count == 0)
                return new List<TPHHItemResponse>();
            var result = _db.TP_HoaHoc
                .Where(x => IDs.Contains(x.ID) && !x.IsDeleted)
                .Select(o => new TPHHItemResponse(
                    o.ID,
                    o.Ma_TPHH,
                    o.Ten_TPHH,
                    o.IsDeleted
                ))
                .ToList();

            return result;
        }
    }
}

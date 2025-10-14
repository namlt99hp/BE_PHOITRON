using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class CTP_RangBuoc_TPHHRepository : BaseRepository<CTP_RangBuoc_TPHH>, ICTP_RangBuoc_TPHHRepository
    {
        public CTP_RangBuoc_TPHHRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<CTP_RangBuoc_TPHH>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == idCongThucPhoi)
                .OrderBy(x => x.Uu_Tien)
                .ToListAsync(ct);
        }

        protected override IQueryable<CTP_RangBuoc_TPHH> ApplySearchFilter(IQueryable<CTP_RangBuoc_TPHH> query, string search)
        {
            return query.Where(x => (x.Ghi_Chu != null && x.Ghi_Chu.Contains(search)));
        }

        protected override IQueryable<CTP_RangBuoc_TPHH> ApplySorting(IQueryable<CTP_RangBuoc_TPHH> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "min_phantram" => isDesc ? query.OrderByDescending(x => x.Min_PhanTram) : query.OrderBy(x => x.Min_PhanTram),
                "max_phantram" => isDesc ? query.OrderByDescending(x => x.Max_PhanTram) : query.OrderBy(x => x.Max_PhanTram),
                "uu_tien" => isDesc ? query.OrderByDescending(x => x.Uu_Tien) : query.OrderBy(x => x.Uu_Tien),
                _ => query.OrderBy(x => x.Uu_Tien)
            };
        }

        public override async Task<(int total, IReadOnlyList<CTP_RangBuoc_TPHH> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<CTP_RangBuoc_TPHH> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => (x.Ghi_Chu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<CTP_RangBuoc_TPHH>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderBy(x => x.Uu_Tien ?? byte.MaxValue);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }
    }
}

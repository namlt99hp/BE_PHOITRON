using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class TP_HoaHocRepository : BaseRepository<TP_HoaHoc>, ITP_HoaHocRepository
    {
        public TP_HoaHocRepository(AppDbContext db) : base(db) { }

        public async Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.Ma_TPHH == maTPHH && !x.Da_Xoa, ct);

        public async Task<IReadOnlyList<TP_HoaHoc>> GetActiveAsync(CancellationToken ct = default)
            => await _set.AsNoTracking()
                .Where(x => !x.Da_Xoa)
                .OrderBy(x => x.Thu_Tu)
                .ToListAsync(ct);

        public override async Task<(int total, IReadOnlyList<TP_HoaHoc> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<TP_HoaHoc> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.Ma_TPHH.Contains(search) ||
                                 (x.Ten_TPHH ?? "").Contains(search) ||
                                 (x.Don_Vi ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<TP_HoaHoc>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderByDescending(x => x.Ngay_Tao);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }

        protected override IQueryable<TP_HoaHoc> ApplySearchFilter(IQueryable<TP_HoaHoc> query, string search)
        {
            return query.Where(x => x.Ma_TPHH.Contains(search) || 
                                  (x.Ten_TPHH != null && x.Ten_TPHH.Contains(search)));
        }

        protected override IQueryable<TP_HoaHoc> ApplySorting(IQueryable<TP_HoaHoc> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "ma_tphh" => isDesc ? query.OrderByDescending(x => x.Ma_TPHH) : query.OrderBy(x => x.Ma_TPHH),
                "ten_tphh" => isDesc ? query.OrderByDescending(x => x.Ten_TPHH) : query.OrderBy(x => x.Ten_TPHH),
                "thu_tu" => isDesc ? query.OrderByDescending(x => x.Thu_Tu) : query.OrderBy(x => x.Thu_Tu),
                _ => query.OrderBy(x => x.Thu_Tu)
            };
        }
    }
}

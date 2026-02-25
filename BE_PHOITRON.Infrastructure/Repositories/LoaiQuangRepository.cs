using System.Linq.Dynamic.Core;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class LoaiQuangRepository : BaseRepository<LoaiQuang>, ILoaiQuangRepository
    {
        public LoaiQuangRepository(AppDbContext db) : base(db) { }

        public async Task<bool> ExistsByCodeAsync(string maLoaiQuang, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.MaLoaiQuang == maLoaiQuang, ct);

        public async Task<IReadOnlyList<LoaiQuang>> GetActiveAsync(CancellationToken ct = default)
            => await _set.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.TenLoaiQuang)
                .ToListAsync(ct);

        public override async Task<(int total, IReadOnlyList<LoaiQuang> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<LoaiQuang> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.MaLoaiQuang.Contains(search) ||
                                 x.TenLoaiQuang.Contains(search) ||
                                 (x.MoTa ?? string.Empty).Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<LoaiQuang>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderBy(x => x.TenLoaiQuang);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }

        protected override IQueryable<LoaiQuang> ApplySearchFilter(IQueryable<LoaiQuang> query, string search)
        {
            return query.Where(x => x.MaLoaiQuang.Contains(search) ||
                                    x.TenLoaiQuang.Contains(search));
        }

        protected override IQueryable<LoaiQuang> ApplySorting(IQueryable<LoaiQuang> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "maLoaiQuang" => isDesc ? query.OrderByDescending(x => x.MaLoaiQuang) : query.OrderBy(x => x.MaLoaiQuang),
                "tenLoaiQuang" => isDesc ? query.OrderByDescending(x => x.TenLoaiQuang) : query.OrderBy(x => x.TenLoaiQuang),
                _ => query.OrderBy(x => x.TenLoaiQuang)
            };
        }
    }
}


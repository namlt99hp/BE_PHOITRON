using System.Linq.Dynamic.Core;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class LoQuangRepository : BaseRepository<LoQuang>, ILoQuangRepository
    {
        public LoQuangRepository(AppDbContext db) : base(db) { }

        public async Task<bool> ExistsByCodeAsync(string maLoQuang, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.MaLoQuang == maLoQuang, ct);

        public async Task<IReadOnlyList<LoQuang>> GetActiveAsync(CancellationToken ct = default)
            => await _set.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync(ct);

        public override async Task<(int total, IReadOnlyList<LoQuang> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<LoQuang> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.MaLoQuang.Contains(search) ||
                                 (x.MoTa ?? string.Empty).Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<LoQuang>(sortBy))
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

        protected override IQueryable<LoQuang> ApplySearchFilter(IQueryable<LoQuang> query, string search)
        {
            return query.Where(x => x.MaLoQuang.Contains(search) ||
                                    (x.MoTa ?? string.Empty).Contains(search));
        }

        protected override IQueryable<LoQuang> ApplySorting(IQueryable<LoQuang> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "maLoQuang" => isDesc ? query.OrderByDescending(x => x.MaLoQuang) : query.OrderBy(x => x.MaLoQuang),
                "ngayTao" => isDesc ? query.OrderByDescending(x => x.NgayTao) : query.OrderBy(x => x.NgayTao),
                _ => query.OrderByDescending(x => x.NgayTao)
            };
        }
    }
}


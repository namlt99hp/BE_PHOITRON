using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class CTP_ChiTiet_QuangRepository : BaseRepository<CTP_ChiTiet_Quang>, ICTP_ChiTiet_QuangRepository
    {
        public CTP_ChiTiet_QuangRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<CTP_ChiTiet_Quang>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Cong_Thuc_Phoi == idCongThucPhoi)
                .OrderBy(x => x.Thu_Tu)
                .ToListAsync(ct);
        }

        public async Task<bool> ValidateTotalPercentageAsync(int idCongThucPhoi, CancellationToken ct = default)
        {
            var totalPercentage = await _set
                .Where(x => x.ID_Cong_Thuc_Phoi == idCongThucPhoi)
                .SumAsync(x => x.Ti_Le_Phan_Tram, ct);
            
            // Allow tolerance of ±0.01%
            return Math.Abs(totalPercentage - 100) <= 0.01m;
        }

        protected override IQueryable<CTP_ChiTiet_Quang> ApplySearchFilter(IQueryable<CTP_ChiTiet_Quang> query, string search)
        {
            return query.Where(x => (x.Ghi_Chu != null && x.Ghi_Chu.Contains(search)));
        }

        protected override IQueryable<CTP_ChiTiet_Quang> ApplySorting(IQueryable<CTP_ChiTiet_Quang> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "ti_le_phan_tram" => isDesc ? query.OrderByDescending(x => x.Ti_Le_Phan_Tram) : query.OrderBy(x => x.Ti_Le_Phan_Tram),
                "thu_tu" => isDesc ? query.OrderByDescending(x => x.Thu_Tu) : query.OrderBy(x => x.Thu_Tu),
                _ => query.OrderBy(x => x.Thu_Tu)
            };
        }

        public override async Task<(int total, IReadOnlyList<CTP_ChiTiet_Quang> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<CTP_ChiTiet_Quang> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => (x.Ghi_Chu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<CTP_ChiTiet_Quang>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderBy(x => x.Thu_Tu ?? int.MaxValue);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }
    }
}

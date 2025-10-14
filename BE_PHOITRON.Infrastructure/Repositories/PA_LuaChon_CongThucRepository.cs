using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class PA_LuaChon_CongThucRepository : BaseRepository<PA_LuaChon_CongThuc>, IPA_LuaChon_CongThucRepository
    {
        public PA_LuaChon_CongThucRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<PA_LuaChon_CongThuc>> GetByPhuongAnAsync(int idPhuongAn, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Phuong_An == idPhuongAn)
                .ToListAsync(ct);
        }

        public async Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default)
        {
            // This will be implemented with recursive CTE to check for circular dependencies
            // For now, return true (no circular dependency)
            // TODO: Implement recursive CTE logic to traverse the recipe tree
            return await Task.FromResult(true);
        }

        protected override IQueryable<PA_LuaChon_CongThuc> ApplySearchFilter(IQueryable<PA_LuaChon_CongThuc> query, string search)
        {
            // This entity doesn't have searchable text fields
            return query;
        }

        protected override IQueryable<PA_LuaChon_CongThuc> ApplySorting(IQueryable<PA_LuaChon_CongThuc> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            var by = sortBy.ToLower();

            // Friendly aliases → property paths
            by = by switch
            {
                "ten_cong_thuc" => "Cong_Thuc_Phoi.Ten_Cong_Thuc",
                "ma_cong_thuc" => "Cong_Thuc_Phoi.Ma_Cong_Thuc",
                "ten_quang" => "Quang_DauRa.Ten_Quang",
                "ma_quang" => "Quang_DauRa.Ma_Quang",
                "ten_phuong_an" => "Phuong_An_Phoi.Ten_Phuong_An",
                _ => sortBy
            };

            if (!string.IsNullOrWhiteSpace(by) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<PA_LuaChon_CongThuc>(by))
            {
                var dir = isDesc ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                return query.OrderBy(cfg, $"{by} {dir}");
            }

            return query.OrderBy(x => x.ID);
        }

        public override async Task<(int total, IReadOnlyList<PA_LuaChon_CongThuc> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            // Build a joinable query without navigation properties
            var q = from pa in _set.AsNoTracking()
                    join cong in _db.Set<Cong_Thuc_Phoi>().AsNoTracking() on pa.ID_Cong_Thuc_Phoi equals cong.ID into congJoin
                    from cong in congJoin.DefaultIfEmpty()
                    join quang in _db.Set<Quang>().AsNoTracking() on pa.ID_Quang_DauRa equals quang.ID into quangJoin
                    from quang in quangJoin.DefaultIfEmpty()
                    join plan in _db.Set<Phuong_An_Phoi>().AsNoTracking() on pa.ID_Phuong_An equals plan.ID into planJoin
                    from plan in planJoin.DefaultIfEmpty()
                    select new { pa, cong, quang, plan };

            if (!string.IsNullOrWhiteSpace(search))
            {
                q = q.Where(x => (x.cong.Ten_Cong_Thuc ?? "").Contains(search) ||
                                 (x.cong.Ma_Cong_Thuc ?? "").Contains(search) ||
                                 (x.quang.Ten_Quang ?? "").Contains(search) ||
                                 (x.quang.Ma_Quang ?? "").Contains(search) ||
                                 (x.plan.Ten_Phuong_An ?? "").Contains(search));
            }

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
                switch (sortBy.ToLower())
                {
                    case "ten_cong_thuc":
                        q = isDesc ? q.OrderByDescending(x => x.cong.Ten_Cong_Thuc) : q.OrderBy(x => x.cong.Ten_Cong_Thuc); break;
                    case "ma_cong_thuc":
                        q = isDesc ? q.OrderByDescending(x => x.cong.Ma_Cong_Thuc) : q.OrderBy(x => x.cong.Ma_Cong_Thuc); break;
                    case "ten_quang":
                        q = isDesc ? q.OrderByDescending(x => x.quang.Ten_Quang) : q.OrderBy(x => x.quang.Ten_Quang); break;
                    case "ma_quang":
                        q = isDesc ? q.OrderByDescending(x => x.quang.Ma_Quang) : q.OrderBy(x => x.quang.Ma_Quang); break;
                    case "ten_phuong_an":
                        q = isDesc ? q.OrderByDescending(x => x.plan.Ten_Phuong_An) : q.OrderBy(x => x.plan.Ten_Phuong_An); break;
                    default:
                        q = q.OrderBy(x => x.pa.ID); break;
                }
            }
            else
            {
                q = q.OrderBy(x => x.pa.ID);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .Select(x => x.pa)
                              .ToListAsync(ct);

            return (total, data);
        }

        public async Task<(int total, IReadOnlyList<PA_LuaChon_CongThuc> data)> SearchPagedAdvancedAsync(
            int page,
            int pageSize,
            int? idPhuongAn = null,
            int? idQuangDauRa = null,
            int? idCongThucPhoi = null,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            // Build join across related tables without navigation properties
            var q = from pa in _set.AsNoTracking()
                    join cong in _db.Set<Cong_Thuc_Phoi>().AsNoTracking() on pa.ID_Cong_Thuc_Phoi equals cong.ID into congJoin
                    from cong in congJoin.DefaultIfEmpty()
                    join quang in _db.Set<Quang>().AsNoTracking() on pa.ID_Quang_DauRa equals quang.ID into quangJoin
                    from quang in quangJoin.DefaultIfEmpty()
                    join plan in _db.Set<Phuong_An_Phoi>().AsNoTracking() on pa.ID_Phuong_An equals plan.ID into planJoin
                    from plan in planJoin.DefaultIfEmpty()
                    select new { pa, cong, quang, plan };

            if (idPhuongAn.HasValue)
                q = q.Where(x => x.pa.ID_Phuong_An == idPhuongAn.Value);
            if (idQuangDauRa.HasValue)
                q = q.Where(x => x.pa.ID_Quang_DauRa == idQuangDauRa.Value);
            if (idCongThucPhoi.HasValue)
                q = q.Where(x => x.pa.ID_Cong_Thuc_Phoi == idCongThucPhoi.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                q = q.Where(x => (x.cong.Ten_Cong_Thuc ?? "").Contains(search) ||
                                 (x.cong.Ma_Cong_Thuc ?? "").Contains(search) ||
                                 (x.quang.Ten_Quang ?? "").Contains(search) ||
                                 (x.quang.Ma_Quang ?? "").Contains(search) ||
                                 (x.plan.Ten_Phuong_An ?? "").Contains(search));
            }

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
                switch (sortBy.ToLower())
                {
                    case "ten_cong_thuc":
                        q = isDesc ? q.OrderByDescending(x => x.cong.Ten_Cong_Thuc) : q.OrderBy(x => x.cong.Ten_Cong_Thuc); break;
                    case "ma_cong_thuc":
                        q = isDesc ? q.OrderByDescending(x => x.cong.Ma_Cong_Thuc) : q.OrderBy(x => x.cong.Ma_Cong_Thuc); break;
                    case "ten_quang":
                        q = isDesc ? q.OrderByDescending(x => x.quang.Ten_Quang) : q.OrderBy(x => x.quang.Ten_Quang); break;
                    case "ma_quang":
                        q = isDesc ? q.OrderByDescending(x => x.quang.Ma_Quang) : q.OrderBy(x => x.quang.Ma_Quang); break;
                    case "ten_phuong_an":
                        q = isDesc ? q.OrderByDescending(x => x.plan.Ten_Phuong_An) : q.OrderBy(x => x.plan.Ten_Phuong_An); break;
                    default:
                        q = q.OrderBy(x => x.pa.ID); break;
                }
            }
            else
            {
                q = q.OrderBy(x => x.pa.ID);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .Select(x => x.pa)
                              .ToListAsync(ct);

            return (total, data);
        }
    }
}

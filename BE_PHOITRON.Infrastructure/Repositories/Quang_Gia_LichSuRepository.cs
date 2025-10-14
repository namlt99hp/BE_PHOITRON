using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class Quang_Gia_LichSuRepository : BaseRepository<Quang_Gia_LichSu>, IQuang_Gia_LichSuRepository
    {
        public Quang_Gia_LichSuRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Quang_Gia_LichSu>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang == idQuang &&
                           x.Hieu_Luc_Tu <= ngayTinh &&
                           (x.Hieu_Luc_Den == null || x.Hieu_Luc_Den > ngayTinh))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Quang_Gia_LichSu>> GetByQuangAsync(int idQuang, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang == idQuang && !x.Da_Xoa)
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .ToListAsync(ct);
        }

        public async Task<Quang_Gia_LichSu?> GetCurrentPriceAsync(int idQuang, DateTimeOffset ngayTinhToan, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang == idQuang && 
                           x.Hieu_Luc_Tu <= ngayTinhToan &&
                           (x.Hieu_Luc_Den == null || x.Hieu_Luc_Den > ngayTinhToan) &&
                           !x.Da_Xoa)
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> HasOverlappingPeriodAsync(int idQuang, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _set.Where(x => x.ID_Quang == idQuang);
            
            if (excludeId.HasValue)
                query = query.Where(x => x.ID != excludeId.Value);

            // Check for overlapping periods
            return await query.AnyAsync(x => 
                (hieuLucDen == null && x.Hieu_Luc_Den == null) || // Both are open-ended
                (hieuLucDen == null && x.Hieu_Luc_Den > hieuLucTu) || // New is open-ended, existing ends after new starts
                (x.Hieu_Luc_Den == null && hieuLucDen > x.Hieu_Luc_Tu) || // Existing is open-ended, new ends after existing starts
                (hieuLucDen.HasValue && x.Hieu_Luc_Den.HasValue && 
                 hieuLucTu < x.Hieu_Luc_Den && hieuLucDen > x.Hieu_Luc_Tu), ct); // Both have end dates and overlap
        }

        protected override IQueryable<Quang_Gia_LichSu> ApplySearchFilter(IQueryable<Quang_Gia_LichSu> query, string search)
        {
            return query.Where(x => (x.Ghi_Chu != null && x.Ghi_Chu.Contains(search)) ||
                                  x.Tien_Te.Contains(search));
        }

        protected override IQueryable<Quang_Gia_LichSu> ApplySorting(IQueryable<Quang_Gia_LichSu> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "hieu_luc_tu" => isDesc ? query.OrderByDescending(x => x.Hieu_Luc_Tu) : query.OrderBy(x => x.Hieu_Luc_Tu),
                "don_gia_usd_1tan" => isDesc ? query.OrderByDescending(x => x.Don_Gia_USD_1Tan) : query.OrderBy(x => x.Don_Gia_USD_1Tan),
                "don_gia_vnd_1tan" => isDesc ? query.OrderByDescending(x => x.Don_Gia_VND_1Tan) : query.OrderBy(x => x.Don_Gia_VND_1Tan),
                "ty_gia_usd_vnd" => isDesc ? query.OrderByDescending(x => x.Ty_Gia_USD_VND) : query.OrderBy(x => x.Ty_Gia_USD_VND),
                _ => query.OrderByDescending(x => x.Hieu_Luc_Tu)
            };
        }

        public override async Task<(int total, IReadOnlyList<Quang_Gia_LichSu> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Quang_Gia_LichSu> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => (x.Ghi_Chu ?? "").Contains(search) || (x.Tien_Te ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<Quang_Gia_LichSu>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderByDescending(x => x.Hieu_Luc_Tu);
            }

            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .ToListAsync(ct);

            return (total, data);
        }
    }
}

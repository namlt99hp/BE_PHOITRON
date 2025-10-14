using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Quang = BE_PHOITRON.Domain.Entities.Quang;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class Quang_TP_PhanTichRepository : BaseRepository<Quang_TP_PhanTich>, IQuang_TP_PhanTichRepository
    {
        public Quang_TP_PhanTichRepository(AppDbContext db) : base(db) { }

        public async Task<IReadOnlyList<Quang_TP_PhanTich>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang == idQuang &&
                           x.Hieu_Luc_Tu <= ngayTinh &&
                           (x.Hieu_Luc_Den == null || x.Hieu_Luc_Den > ngayTinh))
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Quang_TP_PhanTich>> GetByQuangAsync(int idQuang, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(x => x.ID_Quang == idQuang)
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .ToListAsync(ct);
        }

        public async Task<bool> HasOverlappingPeriodAsync(int idQuang, int idTPHH, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _set.Where(x => x.ID_Quang == idQuang && x.ID_TPHH == idTPHH);
            
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

        protected override IQueryable<Quang_TP_PhanTich> ApplySearchFilter(IQueryable<Quang_TP_PhanTich> query, string search)
        {
            return query.Where(x => x.Nguon_Du_Lieu != null && x.Nguon_Du_Lieu.Contains(search) ||
                                  (x.Ghi_Chu != null && x.Ghi_Chu.Contains(search)));
        }

        protected override IQueryable<Quang_TP_PhanTich> ApplySorting(IQueryable<Quang_TP_PhanTich> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "hieu_luc_tu" => isDesc ? query.OrderByDescending(x => x.Hieu_Luc_Tu) : query.OrderBy(x => x.Hieu_Luc_Tu),
                "gia_tri_phantram" => isDesc ? query.OrderByDescending(x => x.Gia_Tri_PhanTram) : query.OrderBy(x => x.Gia_Tri_PhanTram),
                _ => query.OrderByDescending(x => x.Hieu_Luc_Tu)
            };
        }

        public override async Task<(int total, IReadOnlyList<Quang_TP_PhanTich> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Quang_TP_PhanTich> q = _set.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => (x.Nguon_Du_Lieu ?? "").Contains(search) || (x.Ghi_Chu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<Quang_TP_PhanTich>(sortBy))
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

        public async Task<Dictionary<int, decimal>> CalculateTPHHFormulasAsync(int quangId, CancellationToken ct = default)
        {
            // First check if the quang is Gang (Loai_Quang = 2) or Xỉ (Loai_Quang = 4)
            var quang = await _db.Set<Quang>().AsNoTracking()
                .Where(x => x.ID == quangId)
                .Select(x => new { x.Loai_Quang })
                .FirstOrDefaultAsync(ct);
            
            if (quang == null || (quang.Loai_Quang != 2 && quang.Loai_Quang != 4))
            {
                // Return empty dictionary if not Gang or Xỉ
                return new Dictionary<int, decimal>();
            }
            
            // Get all TPHH data for this quang with formulas
            // Only apply formulas for Gang (Loai_Quang = 2) and Xỉ (Loai_Quang = 4)
            var tphhData = await _set.AsNoTracking()
                .Where(x => x.ID_Quang == quangId && 
                           x.IsCalculated == true && 
                           !string.IsNullOrEmpty(x.CalcFormula) &&
                           !x.Da_Xoa)
                .OrderBy(x => x.ThuTuTPHH ?? 0)
                .ToListAsync(ct);
            
            // Build formulas map and initial values
            var formulas = new Dictionary<int, string>();
            var initialValues = new Dictionary<int, decimal>();
            
            foreach (var item in tphhData)
            {
                formulas[item.ID_TPHH] = item.CalcFormula!;
                initialValues[item.ID_TPHH] = item.Gia_Tri_PhanTram;
            }
            
            // For now, return initial values (will implement iterative solving later)
            // TODO: Implement formula evaluation with cross-reference support
            return initialValues;
        }
    }
}

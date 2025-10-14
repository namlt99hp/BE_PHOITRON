using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class QuangRepository : BaseRepository<Quang>, IQuangRepository
    {
        private readonly AppDbContext _dbContext;

        public QuangRepository(AppDbContext db) : base(db)
        {
            _dbContext = db;
        }

        public async Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.Ma_Quang == maQuang && !x.Da_Xoa, ct);

        public async Task<IReadOnlyList<Quang>> GetByLoaiAsync(int loaiQuang, CancellationToken ct = default)
            => await _set.AsNoTracking()
                .Where(x => x.Loai_Quang == loaiQuang && !x.Da_Xoa)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Quang>> GetActiveAsync(CancellationToken ct = default)
            => await _set.AsNoTracking()
                .Where(x => x.Dang_Hoat_Dong && !x.Da_Xoa)
                .ToListAsync(ct);

        public async Task<QuangDetailResponse?> GetDetailByIdAsync(int id, CancellationToken ct = default)
        {
            // Get quặng basic info
            var quang = await _set.AsNoTracking()
                .Where(x => x.ID == id && !x.Da_Xoa)
                .FirstOrDefaultAsync(ct);

            if (quang == null) return null;

            // Get chemical composition
            var tpHoaHocs = await _dbContext.Set<Quang_TP_PhanTich>()
                .AsNoTracking()
                .Where(x => x.ID_Quang == id && !x.Da_Xoa)
                .Join(_dbContext.Set<TP_HoaHoc>(),
                    qt => qt.ID_TPHH,
                    tphh => tphh.ID,
                    (qt, tphh) => new { qt, tphh })
                .OrderBy(x => x.qt.ThuTuTPHH)
                .ThenBy(x => x.tphh.Ma_TPHH)
                .Select(x => new TPHHOfQuangResponse(
                    x.tphh.ID,
                    x.tphh.Ma_TPHH,
                    x.tphh.Ten_TPHH,
                    x.qt.Gia_Tri_PhanTram,
                    x.qt.ThuTuTPHH,
                    x.qt.CalcFormula,
                    x.qt.IsCalculated
                ))
                .ToListAsync(ct);

            // Get current pricing (most recent effective price)
            var currentPrice = await _dbContext.Set<Quang_Gia_LichSu>()
                .AsNoTracking()
                .Where(x => x.ID_Quang == id && !x.Da_Xoa && x.Hieu_Luc_Tu <= DateTimeOffset.Now && (x.Hieu_Luc_Den == null || x.Hieu_Luc_Den >= DateTimeOffset.Now))
                .OrderByDescending(x => x.Hieu_Luc_Tu)
                .FirstOrDefaultAsync(ct);

            QuangGiaDto? giaHienTai = null;
            if (currentPrice != null)
            {
                giaHienTai = new QuangGiaDto(
                    currentPrice.Don_Gia_USD_1Tan,
                    currentPrice.Ty_Gia_USD_VND,
                    currentPrice.Don_Gia_VND_1Tan,
                    currentPrice.Hieu_Luc_Tu
                );
            }

            return new QuangDetailResponse(
                new QuangResponse(
                    quang.ID,
                    quang.Ma_Quang,
                    quang.Ten_Quang ?? string.Empty,
                    quang.Loai_Quang,
                    quang.Dang_Hoat_Dong,
                    quang.Da_Xoa,
                    quang.Ghi_Chu,
                    quang.Ngay_Tao,
                    quang.Nguoi_Tao,
                    quang.Ngay_Sua,
                    quang.Nguoi_Sua,
                    currentPrice?.Don_Gia_USD_1Tan,
                    currentPrice?.Don_Gia_VND_1Tan,
                    currentPrice?.Ty_Gia_USD_VND,
                    currentPrice?.Hieu_Luc_Tu,
                    currentPrice?.Tien_Te
                ),
                tpHoaHocs,
                giaHienTai
            );
        }

        
        public async Task<(int total, IReadOnlyList<QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, int? loaiQuang = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Quang> q = _set.AsNoTracking();

            q = q.Where(x => x.ID_Quang_Gang == null);
            if (loaiQuang.HasValue)
            {
                q = q.Where(x => x.Loai_Quang == loaiQuang.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(x => x.Ma_Quang.Contains(search) ||
                                 (x.Ten_Quang ?? "").Contains(search) ||
                                 (x.Ghi_Chu ?? "").Contains(search));

            var total = await q.CountAsync(ct);

            if (!string.IsNullOrWhiteSpace(sortBy) && Infrastructure.Shared.CheckValidPropertyPath.IsValidPropertyPath<Quang>(sortBy))
            {
                var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "descending" : "ascending";
                var cfg = new ParsingConfig { IsCaseSensitive = false };
                q = q.OrderBy(cfg, $"{sortBy} {dir}");
            }
            else
            {
                q = q.OrderByDescending(x => x.Ngay_Tao);
            }

            var now = DateTimeOffset.Now;
            var data = await q.Skip(page * pageSize)
                              .Take(pageSize)
                              .Select(x => new QuangResponse(
                                  x.ID,
                                  x.Ma_Quang,
                                  x.Ten_Quang ?? string.Empty,
                                  x.Loai_Quang,
                                  x.Dang_Hoat_Dong,
                                  x.Da_Xoa,
                                  x.Ghi_Chu,
                                  x.Ngay_Tao,
                                  x.Nguoi_Tao,
                                  x.Ngay_Sua,
                                  x.Nguoi_Sua,
                                  _dbContext.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Don_Gia_USD_1Tan)
                                    .FirstOrDefault(),
                                  _dbContext.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Don_Gia_VND_1Tan)
                                    .FirstOrDefault(),
                                  _dbContext.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Ty_Gia_USD_VND)
                                    .FirstOrDefault(),
                                  _dbContext.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => (DateTimeOffset?)p.Hieu_Luc_Tu)
                                    .FirstOrDefault(),
                                    _dbContext.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Tien_Te)
                                    .FirstOrDefault()
                              ))
                              .ToListAsync(ct);

            return (total, data);
        }

        protected override IQueryable<Quang> ApplySearchFilter(IQueryable<Quang> query, string search)
        {
            return query.Where(x => x.Ma_Quang.Contains(search) || 
                                  (x.Ten_Quang != null && x.Ten_Quang.Contains(search)));
        }

        protected override IQueryable<Quang> ApplySorting(IQueryable<Quang> query, string sortBy, string? sortDir)
        {
            var isDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            
            return sortBy.ToLower() switch
            {
                "ma_quang" => isDesc ? query.OrderByDescending(x => x.Ma_Quang) : query.OrderBy(x => x.Ma_Quang),
                "ten_quang" => isDesc ? query.OrderByDescending(x => x.Ten_Quang) : query.OrderBy(x => x.Ten_Quang),
                "loai_quang" => isDesc ? query.OrderByDescending(x => x.Loai_Quang) : query.OrderBy(x => x.Loai_Quang),
                "ngay_tao" => isDesc ? query.OrderByDescending(x => x.Ngay_Tao) : query.OrderBy(x => x.Ngay_Tao),
                _ => query.OrderByDescending(x => x.Ngay_Tao)
            };
        }



        public async Task<int> UpsertWithThanhPhanAsync(QuangUpsertWithThanhPhanDto dto, CancellationToken ct = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                Quang quangEntity;

                if (dto.ID.HasValue && dto.ID.Value > 0)
                {
                    quangEntity = await _set.FindAsync(dto.ID.Value, ct) ?? throw new InvalidOperationException($"Không tìm thấy quặng với ID {dto.ID.Value}");

                    if (await ExistsByCodeAsync(dto.Ma_Quang, ct))
                    {
                        var existingEntity = await _set.FirstOrDefaultAsync(x => x.Ma_Quang == dto.Ma_Quang && x.ID != dto.ID.Value, ct);
                        if (existingEntity != null)
                            throw new InvalidOperationException($"Mã quặng '{dto.Ma_Quang}' đã tồn tại.");
                    }

                    quangEntity.Ma_Quang = dto.Ma_Quang;
                    quangEntity.Ten_Quang = dto.Ten_Quang;
                    quangEntity.Loai_Quang = dto.Loai_Quang;
                    quangEntity.Dang_Hoat_Dong = dto.Dang_Hoat_Dong;
                    quangEntity.Ghi_Chu = dto.Ghi_Chu;
                    quangEntity.Ngay_Sua = DateTimeOffset.Now;
                    quangEntity.Nguoi_Sua = null;
                    quangEntity.ID_Quang_Gang = dto.ID_Quang_Gang;

                    _set.Update(quangEntity);
                }
                else
                {
                    if (await ExistsByCodeAsync(dto.Ma_Quang, ct))
                        throw new InvalidOperationException($"Mã quặng '{dto.Ma_Quang}' đã tồn tại.");

                    quangEntity = new Quang
                    {
                        Ma_Quang = dto.Ma_Quang,
                        Ten_Quang = dto.Ten_Quang,
                        Loai_Quang = dto.Loai_Quang,
                        Dang_Hoat_Dong = dto.Dang_Hoat_Dong,
                        Da_Xoa = false,
                        Ghi_Chu = dto.Ghi_Chu,
                        Ngay_Tao = DateTimeOffset.Now,
                        Nguoi_Tao = null,
                        ID_Quang_Gang = dto.ID_Quang_Gang
                    };

                    await _set.AddAsync(quangEntity, ct);
                }

                await _dbContext.SaveChangesAsync(ct);

                // Optimized upsert for chemical compositions
                if (dto.ThanhPhanHoaHoc != null)
                {
                    var currentDate = DateTimeOffset.Now;
                    var existingCompositionsList = await _dbContext.Set<Quang_TP_PhanTich>()
                        .Where(x => x.ID_Quang == quangEntity.ID)
                        .ToListAsync(ct);
                    var existingCompositions = existingCompositionsList.ToLookup(x => x.ID_TPHH, x => x);

                    var dtoIds = dto.ThanhPhanHoaHoc.Select(x => x.ID_TPHH).ToHashSet();

                    // Batch operations for better performance
                    var toUpdate = new List<Quang_TP_PhanTich>();
                    var toAdd = new List<Quang_TP_PhanTich>();
                    var toRemove = new List<Quang_TP_PhanTich>();

                    // Process DTO items
                    foreach (var tp in dto.ThanhPhanHoaHoc)
                    {
                        var existingGroup = existingCompositions[tp.ID_TPHH];
                        var existing = existingGroup.FirstOrDefault();
                        
                        if (existing != null)
                        {
                            // Update existing - only if values actually changed
                            if (existing.Gia_Tri_PhanTram != tp.Gia_Tri_PhanTram ||
                                existing.ThuTuTPHH != (tp.ThuTuTPHH ?? existing.ThuTuTPHH) ||
                                existing.KhoiLuong != tp.KhoiLuong ||
                                existing.CalcFormula != tp.CalcFormula ||
                                existing.IsCalculated != tp.IsCalculated)
                            {
                                existing.Gia_Tri_PhanTram = tp.Gia_Tri_PhanTram;
                                existing.ThuTuTPHH = tp.ThuTuTPHH ?? existing.ThuTuTPHH;
                                existing.KhoiLuong = tp.KhoiLuong;
                                existing.CalcFormula = tp.CalcFormula;
                                existing.IsCalculated = tp.IsCalculated;
                                toUpdate.Add(existing);
                            }
                        }
                        else
                        {
                            // Add new
                            toAdd.Add(new Quang_TP_PhanTich
                            {
                                ID_Quang = quangEntity.ID,
                                ID_TPHH = tp.ID_TPHH,
                                Gia_Tri_PhanTram = tp.Gia_Tri_PhanTram,
                                ThuTuTPHH = tp.ThuTuTPHH ?? 1,
                                KhoiLuong = tp.KhoiLuong,
                                CalcFormula = tp.CalcFormula,
                                IsCalculated = tp.IsCalculated,
                                Hieu_Luc_Tu = currentDate,
                                Hieu_Luc_Den = null,
                                Nguon_Du_Lieu = "Phòng thí nghiệm",
                                Ghi_Chu = null,
                                Da_Xoa = false
                            });
                        }
                    }

                    // Find items to remove (in DB but not in DTO)
                    toRemove = existingCompositions
                        .Where(x => !dtoIds.Contains(x.Key))
                        .SelectMany(x => x)
                        .ToList();

                    // Execute batch operations
                    if (toUpdate.Any())
                    {
                        _dbContext.Set<Quang_TP_PhanTich>().UpdateRange(toUpdate);
                    }
                    if (toAdd.Any())
                    {
                        await _dbContext.Set<Quang_TP_PhanTich>().AddRangeAsync(toAdd, ct);
                    }
                    if (toRemove.Any())
                    {
                        _dbContext.Set<Quang_TP_PhanTich>().RemoveRange(toRemove);
                    }
                }

                // Handle pricing information (required for purchased ore)
                if (dto.Loai_Quang == 0 && dto.Gia == null)
                {
                    throw new InvalidOperationException("Quặng mua về phải có thông tin giá cả");
                }

                if (dto.Gia != null)
                {
                    // Basic validation only - no range constraints
                    if (dto.Gia.Gia_USD_1Tan <= 0)
                        throw new InvalidOperationException("Giá USD phải lớn hơn 0");
                    
                    if (dto.Gia.Ty_Gia_USD_VND <= 0)
                        throw new InvalidOperationException("Tỷ giá USD/VND phải lớn hơn 0");

                    if (dto.Gia.Gia_VND_1Tan <= 0)
                        throw new InvalidOperationException("Giá VND phải lớn hơn 0");

                    // Calculate VND price if not provided or validate if provided
                    decimal giaVNDCalculated = dto.Gia.Gia_USD_1Tan * dto.Gia.Ty_Gia_USD_VND;
                    if (Math.Abs(dto.Gia.Gia_VND_1Tan - giaVNDCalculated) > 0.01m)
                        throw new InvalidOperationException($"Giá VND không khớp với tính toán: USD {dto.Gia.Gia_USD_1Tan} × {dto.Gia.Ty_Gia_USD_VND} = {giaVNDCalculated:N2} VND");

                    // UPSERT logic: Update existing or create new price record
                    var existingPrice = await _dbContext.Set<Quang_Gia_LichSu>()
                        .FirstOrDefaultAsync(x => x.ID_Quang == quangEntity.ID && !x.Da_Xoa, ct);

                    if (existingPrice != null)
                    {
                        // UPDATE existing price record
                        existingPrice.Don_Gia_USD_1Tan = dto.Gia.Gia_USD_1Tan;
                        existingPrice.Don_Gia_VND_1Tan = dto.Gia.Gia_VND_1Tan;
                        existingPrice.Ty_Gia_USD_VND = dto.Gia.Ty_Gia_USD_VND;
                        existingPrice.Tien_Te = "USD";
                        existingPrice.Hieu_Luc_Tu = dto.Gia.Ngay_Chon_TyGia;
                        existingPrice.Hieu_Luc_Den = null; // Keep as current price
                        
                        _dbContext.Set<Quang_Gia_LichSu>().Update(existingPrice);
                    }
                    else
                    {
                        // INSERT new price record (for new quặng)
                        var newPrice = new Quang_Gia_LichSu
                        {
                            ID_Quang = quangEntity.ID,
                            Don_Gia_USD_1Tan = dto.Gia.Gia_USD_1Tan,
                            Don_Gia_VND_1Tan = dto.Gia.Gia_VND_1Tan,
                            Ty_Gia_USD_VND = dto.Gia.Ty_Gia_USD_VND,
                            Tien_Te = "USD",
                            Hieu_Luc_Tu = dto.Gia.Ngay_Chon_TyGia,
                            Hieu_Luc_Den = null, // No end date for current price
                            Ghi_Chu = null,
                            Da_Xoa = false
                        };

                        await _dbContext.Set<Quang_Gia_LichSu>().AddAsync(newPrice, ct);
                    }
                }

                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return quangEntity.ID;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<IReadOnlyList<OreChemistryBatchItem>> GetOreChemistryBatchAsync(IReadOnlyList<int> quangIds, CancellationToken ct = default)
        {
        if (quangIds == null || quangIds.Count == 0) return Array.Empty<OreChemistryBatchItem>();

        var query = from q in _dbContext.Set<Quang>().AsNoTracking()
                    where quangIds.Contains(q.ID) && !q.Da_Xoa
                    select new
                    {
                        q.ID,
                        q.Ten_Quang
                    };

        var quangs = await query.ToListAsync(ct);

        var tphh = await (from tp in _dbContext.Set<Quang_TP_PhanTich>().AsNoTracking()
                          where quangIds.Contains(tp.ID_Quang) && !tp.Da_Xoa
                          group tp by tp.ID_Quang into g
                          select new
                          {
                              IdQuang = g.Key,
                              Items = g.Select(x => new { x.ID_TPHH, x.Gia_Tri_PhanTram, x.ThuTuTPHH }).ToList()
                          }).ToListAsync(ct);

        var tphhByQuang = tphh.ToDictionary(
            x => x.IdQuang,
            x => (IReadOnlyList<TPHHOfQuangMinimal>) x.Items
                    .OrderBy(i => i.ThuTuTPHH)
                    .ThenBy(i => i.ID_TPHH)
                    .Select(i => new TPHHOfQuangMinimal(i.ID_TPHH, i.Gia_Tri_PhanTram, i.ThuTuTPHH))
                    .ToList()
        );

        var result = quangs.Select(q => new OreChemistryBatchItem(
            new QuangMinimal(q.ID, q.Ten_Quang ?? string.Empty),
            tphhByQuang.TryGetValue(q.ID, out var list) ? list : Array.Empty<TPHHOfQuangMinimal>()
        )).ToList();

        return result;
    }

        public async Task<IReadOnlyList<FormulaByOutputOreResponse>> GetFormulasByOutputOreIdsAsync(IReadOnlyList<int> outputOreIds, CancellationToken ct = default)
        {
            if (outputOreIds == null || outputOreIds.Count == 0) return Array.Empty<FormulaByOutputOreResponse>();

            var formulas = await _dbContext.Set<Cong_Thuc_Phoi>().AsNoTracking()
                .Where(f => outputOreIds.Contains(f.ID_Quang_DauRa) && !f.Da_Xoa)
                .ToListAsync(ct);

            var formulaIds = formulas.Select(f => f.ID).ToList();

            var details = await _dbContext.Set<CTP_ChiTiet_Quang>().AsNoTracking()
                .Where(x => formulaIds.Contains(x.ID_Cong_Thuc_Phoi) && !x.Da_Xoa)
                .Join(_dbContext.Set<Quang>().AsNoTracking(), a => a.ID_Quang_DauVao, q => q.ID,
                    (a, q) => new { a.ID_Cong_Thuc_Phoi, q.ID, q.Ma_Quang, q.Ten_Quang, q.Loai_Quang, a.Ti_Le_Phan_Tram })
                .ToListAsync(ct);

            var now = DateTimeOffset.Now;
            var prices = await _dbContext.Set<Quang_Gia_LichSu>().AsNoTracking()
                .Where(p => !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                .GroupBy(p => p.ID_Quang)
                .Select(g => g.OrderByDescending(x => x.Hieu_Luc_Tu).First())
                .ToListAsync(ct);
            var priceMap = prices.ToDictionary(p => p.ID_Quang, p => p);

            var result = new List<FormulaByOutputOreResponse>();
            foreach (var f in formulas)
            {
                var items = details.Where(d => d.ID_Cong_Thuc_Phoi == f.ID)
                    .Select(d => {
                        priceMap.TryGetValue(d.ID, out var p);
                        return new FormulaItem(
                            d.ID,
                            d.Ma_Quang ?? string.Empty,
                            d.Ten_Quang ?? string.Empty,
                            d.Loai_Quang,
                            p?.Don_Gia_USD_1Tan ?? 0,
                            p?.Ty_Gia_USD_VND ?? 0,
                            p?.Don_Gia_VND_1Tan ?? 0,
                            d.Ti_Le_Phan_Tram
                        );
                    })
                    // Sort: mixed ores (Loai_Quang = 1) first, then by ID for stability
                    .OrderBy(x => x.Loai_Quang == 1 ? 0 : 1)
                    .ThenBy(x => x.Id)
                    .ToList();

                result.Add(new FormulaByOutputOreResponse(
                    f.ID_Quang_DauRa,
                    f.ID,
                    f.Ma_Cong_Thuc,
                    f.Ten_Cong_Thuc ?? string.Empty,
                    f.Hieu_Luc_Tu,
                    items
                ));
            }

            return result;
        }

        public async Task<int?> GetSlagIdByGangIdAsync(int gangId, CancellationToken ct = default)
        {
            return await _dbContext.Set<Quang>()
                .AsNoTracking()
                .Where(x => x.Loai_Quang == (int)Domain.Entities.LoaiQuang.Xi && x.ID_Quang_Gang == gangId && !x.Da_Xoa)
                .Select(x => (int?)x.ID)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(QuangDetailResponse? gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryByPlanAsync(int planId, CancellationToken ct = default)
        {
            // Get gang and slag result ores for this plan from PA_Quang_KQ
            var quangKetQua = await _dbContext.PA_Quang_KQ
                .AsNoTracking()
                .Where(x => x.ID_PhuongAn == planId)
                .Join(_dbContext.Quang.AsNoTracking(), 
                    pa => pa.ID_Quang, 
                    q => q.ID, 
                    (pa, q) => new { 
                        ID_Quang = q.ID, 
                        LoaiQuang = q.Loai_Quang
                    })
                .ToListAsync(ct);

            QuangDetailResponse? gang = null;
            QuangDetailResponse? slag = null;

            foreach (var q in quangKetQua)
            {
                var detail = await GetDetailByIdAsync(q.ID_Quang, ct);
                if (detail != null)
                {
                    if (q.LoaiQuang == 2) // Gang
                        gang = detail;
                    else if (q.LoaiQuang == 4) // Xỉ
                        slag = detail;
                }
            }

            return (gang, slag);
        }

        public async Task<int> UpsertKetQuaWithThanhPhanAsync(QuangKetQuaUpsertDto dto, CancellationToken ct = default)
        {
            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                Quang entity;
                if (dto.ID is null or 0)
                {
                    // Create new result ore
                    entity = new Quang
                    {
                        Ma_Quang = dto.Ma_Quang,
                        Ten_Quang = dto.Ten_Quang,
                        Loai_Quang = dto.Loai_Quang,
                        Dang_Hoat_Dong = dto.Dang_Hoat_Dong,
                        Ghi_Chu = dto.Ghi_Chu,
                        Da_Xoa = false,
                        Ngay_Tao = DateTime.Now,
                        ID_Quang_Gang = dto.ID_Quang_Gang
                    };
                    _db.Quang.Add(entity);
                    await _db.SaveChangesAsync(ct);
                }
                else
                {
                    // Update existing result ore
                    entity = await _db.Quang.FindAsync(dto.ID.Value);
                    if (entity == null) throw new ArgumentException($"Quặng kết quả {dto.ID} không tồn tại");

                    entity.Ma_Quang = dto.Ma_Quang;
                    entity.Ten_Quang = dto.Ten_Quang;
                    entity.Loai_Quang = dto.Loai_Quang;
                    entity.Dang_Hoat_Dong = dto.Dang_Hoat_Dong;
                    entity.Ghi_Chu = dto.Ghi_Chu;
                    entity.ID_Quang_Gang = dto.ID_Quang_Gang;
                    entity.Ngay_Sua = DateTimeOffset.Now;

                    _db.Quang.Update(entity);
                    await _db.SaveChangesAsync(ct);
                }

                // Upsert chemical composition using optimized logic
                await UpsertChemicalCompositionAsync(entity.ID, dto.ThanhPhanHoaHoc, ct);

                // Upsert PA_Quang_KQ mapping
                await UpsertPaQuangKqMappingAsync(dto.ID_PhuongAn, entity.ID, dto.Loai_Quang, ct);

                await tx.CommitAsync(ct);
                return entity.ID;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task UpsertPaQuangKqMappingAsync(int idPhuongAn, int idQuang, int loaiQuang, CancellationToken ct = default)
        {
            // Check if mapping already exists
            var existingMapping = await _db.PA_Quang_KQ
                .FirstOrDefaultAsync(x => x.ID_PhuongAn == idPhuongAn && x.LoaiQuang == loaiQuang, ct);

            if (existingMapping != null)
            {
                // Update existing mapping
                existingMapping.ID_Quang = idQuang;
                _db.PA_Quang_KQ.Update(existingMapping);
            }
            else
            {
                // Create new mapping
                var newMapping = new PA_Quang_KQ
                {
                    ID_PhuongAn = idPhuongAn,
                    ID_Quang = idQuang,
                    LoaiQuang = loaiQuang
                };
                _db.PA_Quang_KQ.Add(newMapping);
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task UpsertChemicalCompositionAsync(int quangId, IReadOnlyList<QuangThanhPhanHoaHocDto> thanhPhanHoaHoc, CancellationToken ct = default)
        {
            var currentDate = DateTimeOffset.Now;
            var existingCompositionsList = await _db.Quang_TP_PhanTich
                .Where(x => x.ID_Quang == quangId)
                .ToListAsync(ct);
            var existingCompositions = existingCompositionsList.ToLookup(x => x.ID_TPHH, x => x);

            var dtoIds = thanhPhanHoaHoc.Select(x => x.ID_TPHH).ToHashSet();

            // Batch operations for better performance
            var toUpdate = new List<Quang_TP_PhanTich>();
            var toAdd = new List<Quang_TP_PhanTich>();
            var toRemove = new List<Quang_TP_PhanTich>();

            // Process DTO items
            foreach (var tp in thanhPhanHoaHoc)
            {
                var existingGroup = existingCompositions[tp.ID_TPHH];
                var existing = existingGroup.FirstOrDefault();
                
                if (existing != null)
                {
                    // Update existing - only if values actually changed
                    if (existing.Gia_Tri_PhanTram != tp.Gia_Tri_PhanTram ||
                        existing.ThuTuTPHH != (tp.ThuTuTPHH ?? existing.ThuTuTPHH) ||
                        existing.KhoiLuong != tp.KhoiLuong ||
                        existing.CalcFormula != tp.CalcFormula ||
                        existing.IsCalculated != tp.IsCalculated)
                    {
                        existing.Gia_Tri_PhanTram = tp.Gia_Tri_PhanTram;
                        existing.ThuTuTPHH = tp.ThuTuTPHH ?? existing.ThuTuTPHH;
                        existing.KhoiLuong = tp.KhoiLuong;
                        existing.CalcFormula = tp.CalcFormula;
                        existing.IsCalculated = tp.IsCalculated;
                        toUpdate.Add(existing);
                    }
                }
                else
                {
                    // Add new
                    toAdd.Add(new Quang_TP_PhanTich
                    {
                        ID_Quang = quangId,
                        ID_TPHH = tp.ID_TPHH,
                        Gia_Tri_PhanTram = tp.Gia_Tri_PhanTram,
                        ThuTuTPHH = tp.ThuTuTPHH ?? 1,
                        KhoiLuong = tp.KhoiLuong,
                        CalcFormula = tp.CalcFormula,
                        IsCalculated = tp.IsCalculated,
                        Hieu_Luc_Tu = currentDate,
                        Hieu_Luc_Den = null,
                        Nguon_Du_Lieu = "Phòng thí nghiệm",
                        Ghi_Chu = null,
                        Da_Xoa = false
                    });
                }
            }

            // Find items to remove (in DB but not in DTO)
            toRemove = existingCompositions
                .Where(x => !dtoIds.Contains(x.Key))
                .SelectMany(x => x)
                .ToList();

            // Execute batch operations
            if (toUpdate.Any())
            {
                _db.Quang_TP_PhanTich.UpdateRange(toUpdate);
            }
            if (toAdd.Any())
            {
                await _db.Quang_TP_PhanTich.AddRangeAsync(toAdd, ct);
            }
            if (toRemove.Any())
            {
                _db.Quang_TP_PhanTich.RemoveRange(toRemove);
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}

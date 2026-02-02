using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Domain.Entities;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class QuangRepository : BaseRepository<Quang>, IQuangRepository
    {
        public QuangRepository(AppDbContext db) : base(db) 
        {
        }

        public async Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default)
            => await _set.AnyAsync(x => x.Ma_Quang == maQuang && !x.Da_Xoa, ct);

        public async Task<bool> ExistsByCodeOrNameAsync(string maQuang, string? tenQuang, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _set.Where(x => !x.Da_Xoa);
            
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.ID != excludeId.Value);
            }

            // Check if mã quặng exists
            var maExists = await query.AnyAsync(x => x.Ma_Quang == maQuang, ct);
            if (maExists) return true;

            // Check if tên quặng exists (if provided)
            if (!string.IsNullOrWhiteSpace(tenQuang))
            {
                var tenExists = await query.AnyAsync(x => x.Ten_Quang != null && x.Ten_Quang.Trim().ToLower() == tenQuang.Trim().ToLower(), ct);
                if (tenExists) return true;
            }

            return false;
        }

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
            var tpHoaHocs = await _db.Set<Quang_TP_PhanTich>()
                .AsNoTracking()
                .Where(x => x.ID_Quang == id && !x.Da_Xoa)
                .Join(_db.Set<TP_HoaHoc>(),
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
            var currentPrice = await _db.Set<Quang_Gia_LichSu>()
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
                    currentPrice?.Tien_Te,
                    quang.ID_Quang_Gang
                ),
                tpHoaHocs,
                giaHienTai
            );
        }

        
        public async Task<(int total, IReadOnlyList<QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, int[]? loaiQuang = null, bool? isGangTarget = null, CancellationToken ct = default)
        {
            page = page < 0 ? 0 : page;
            pageSize = pageSize <= 0 || pageSize > 200 ? 20 : pageSize;

            IQueryable<Quang> q = _set.AsNoTracking();

            // Filter by gang target if specified
            if (isGangTarget.HasValue)
            {
                if (isGangTarget.Value)
                {
                    // Only show gang target ores (ID_Quang_Gang = null)
                    q = q.Where(x => x.ID_Quang_Gang == null);
                }
                else
                {
                    // Show all ores including result ores
                    // No additional filtering needed
                }
            }
            else
            {
                // Default behavior: exclude result ores (ID_Quang_Gang != null)
                q = q.Where(x => x.ID_Quang_Gang == null);
            }

            if (loaiQuang != null && loaiQuang.Length > 0)
            {
                q = q.Where(x => loaiQuang.Contains(x.Loai_Quang));
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
                                  _db.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Don_Gia_USD_1Tan)
                                    .FirstOrDefault(),
                                  _db.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Don_Gia_VND_1Tan)
                                    .FirstOrDefault(),
                                  _db.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Ty_Gia_USD_VND)
                                    .FirstOrDefault(),
                                  _db.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => (DateTimeOffset?)p.Hieu_Luc_Tu)
                                    .FirstOrDefault(),
                                    _db.Set<Quang_Gia_LichSu>()
                                    .Where(p => p.ID_Quang == x.ID && !p.Da_Xoa && p.Hieu_Luc_Tu <= now && (p.Hieu_Luc_Den == null || p.Hieu_Luc_Den >= now))
                                    .OrderByDescending(p => p.Hieu_Luc_Tu)
                                    .Select(p => p.Tien_Te)
                                    .FirstOrDefault(),
                                    x.ID_Quang_Gang
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
            var hasExternalTransaction = _db.Database.CurrentTransaction != null;
            IDbContextTransaction? transaction = null;
            if (!hasExternalTransaction)
            {
                transaction = await _db.Database.BeginTransactionAsync(ct);
            }
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
                    quangEntity.Nguoi_Sua = dto.Nguoi_Tao;

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
                        Nguoi_Tao = dto.Nguoi_Tao,
                        ID_Quang_Gang = dto.ID_Quang_Gang
                    };

                    await _set.AddAsync(quangEntity, ct);
                }

                var isGangTarget = quangEntity.Loai_Quang == (int)Domain.Entities.LoaiQuang.Gang && quangEntity.ID_Quang_Gang == null;
                var isSlagOfGangTemplate = quangEntity.Loai_Quang == (int)Domain.Entities.LoaiQuang.Xi && quangEntity.ID_Quang_Gang.HasValue;
                var shouldSaveTemplate = dto.SaveAsTemplate && (isGangTarget || isSlagOfGangTemplate);
                quangEntity.Is_Template = shouldSaveTemplate;

                await _db.SaveChangesAsync(ct);

                // Optimized upsert for chemical compositions
                if (dto.ThanhPhanHoaHoc != null)
                {
                    var currentDate = DateTimeOffset.Now;
                    var existingCompositionsList = await _db.Set<Quang_TP_PhanTich>()
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
                            existing.Is_Template = shouldSaveTemplate;
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
                                Da_Xoa = false,
                                Is_Template = shouldSaveTemplate
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
                        _db.Set<Quang_TP_PhanTich>().UpdateRange(toUpdate);
                    }
                    if (toAdd.Any())
                    {
                        await _db.Set<Quang_TP_PhanTich>().AddRangeAsync(toAdd, ct);
                    }
                    if (toRemove.Any())
                    {
                        _db.Set<Quang_TP_PhanTich>().RemoveRange(toRemove);
                    }
                }

                await UpdateTemplateConfigAsync(quangEntity.ID, shouldSaveTemplate ? dto.TemplateConfig : null, dto.Nguoi_Tao, ct);

                // Handle pricing information - không bắt buộc, cho phép giá = 0 (nguyên liệu xoay vòng)
                if (dto.Gia != null)
                {
                    // Không validate giá phải > 0, cho phép giá = 0
                    // Chỉ validate tính toán nếu cả giá USD và tỷ giá đều > 0
                    if (dto.Gia.Gia_USD_1Tan > 0 && dto.Gia.Ty_Gia_USD_VND > 0)
                    {
                        // Calculate VND price if not provided or validate if provided
                        decimal giaVNDCalculated = dto.Gia.Gia_USD_1Tan * dto.Gia.Ty_Gia_USD_VND;
                        if (Math.Abs(dto.Gia.Gia_VND_1Tan - giaVNDCalculated) > 0.01m)
                            throw new InvalidOperationException($"Giá VND không khớp với tính toán: USD {dto.Gia.Gia_USD_1Tan} × {dto.Gia.Ty_Gia_USD_VND} = {giaVNDCalculated:N2} VND");
                    }

                    // UPSERT logic: Update existing or create new price record
                    var existingPrice = await _db.Set<Quang_Gia_LichSu>()
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
                        
                        _db.Set<Quang_Gia_LichSu>().Update(existingPrice);
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

                        await _db.Set<Quang_Gia_LichSu>().AddAsync(newPrice, ct);
                    }
                }

                await _db.SaveChangesAsync(ct);

                if (!hasExternalTransaction && transaction is not null)
                {
                    await transaction.CommitAsync(ct);
                }
                return quangEntity.ID;
            }
            catch
            {
                if (!hasExternalTransaction && transaction is not null)
                {
                    await transaction.RollbackAsync(ct);
                }
                throw;
            }
            finally
            {
                if (!hasExternalTransaction && transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        public async Task<IReadOnlyList<OreChemistryBatchItem>> GetOreChemistryBatchAsync(IReadOnlyList<int> quangIds, CancellationToken ct = default)
        {
        if (quangIds == null || quangIds.Count == 0) return Array.Empty<OreChemistryBatchItem>();

        var query = from q in _db.Set<Quang>().AsNoTracking()
                    where quangIds.Contains(q.ID) && !q.Da_Xoa
                    select new
                    {
                        q.ID,
                        q.Ten_Quang
                    };

        var quangs = await query.ToListAsync(ct);

        var tphh = await (from tp in _db.Set<Quang_TP_PhanTich>().AsNoTracking()
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

            var formulas = await _db.Set<Cong_Thuc_Phoi>().AsNoTracking()
                .Where(f => outputOreIds.Contains(f.ID_Quang_DauRa) && !f.Da_Xoa)
                .ToListAsync(ct);

            var formulaIds = formulas.Select(f => f.ID).ToList();

            var details = await _db.Set<CTP_ChiTiet_Quang>().AsNoTracking()
                .Where(x => formulaIds.Contains(x.ID_Cong_Thuc_Phoi) && !x.Da_Xoa)
                .Join(_db.Set<Quang>().AsNoTracking(), a => a.ID_Quang_DauVao, q => q.ID,
                    (a, q) => new { a.ID_Cong_Thuc_Phoi, q.ID, q.Ma_Quang, q.Ten_Quang, q.Loai_Quang, a.Ti_Le_Phan_Tram })
                .ToListAsync(ct);

            var now = DateTimeOffset.Now;
            var prices = await _db.Set<Quang_Gia_LichSu>().AsNoTracking()
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
                    // Sort: mixed ores (Loai_Quang = 1 hoặc 7) first, then by ID for stability
                    .OrderBy(x => (x.Loai_Quang == 1 || x.Loai_Quang == 7) ? 0 : 1)
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
            return await _db.Set<Quang>()
                .AsNoTracking()
                .Where(x => x.Loai_Quang == (int)Domain.Entities.LoaiQuang.Xi && x.ID_Quang_Gang == gangId && !x.Da_Xoa)
                .Select(x => (int?)x.ID)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(QuangDetailResponse? gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryByPlanAsync(int planId, CancellationToken ct = default)
        {
            // Get gang and slag result ores for this plan from PA_Quang_KQ
            var quangKetQua = await _db.PA_Quang_KQ
                .AsNoTracking()
                .Where(x => x.ID_PhuongAn == planId)
                .Join(_db.Quang.AsNoTracking(), 
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

        public async Task<QuangDetailResponse?> GetLatestGangTargetAsync(CancellationToken ct = default)
        {
            // Lấy gang đích được tạo gần nhất (loại = 2, ID_Quang_Gang = null)
            var latestGang = await _set.AsNoTracking()
                .Where(x => x.Loai_Quang == 2 // Gang
                         && x.ID_Quang_Gang == null // Gang đích (không phải gang kết quả)
                         && !x.Da_Xoa)
                .OrderByDescending(x => x.Ngay_Tao)
                .FirstOrDefaultAsync(ct);

            if (latestGang == null) return null;

            // Lấy đầy đủ thông tin bằng GetDetailByIdAsync
            return await GetDetailByIdAsync(latestGang.ID, ct);
        }

        public async Task<GangTemplateConfigResponse?> GetGangTemplateConfigAsync(int? gangId = null, CancellationToken ct = default)
        {
            var gangEntity = await FindGangTemplateEntityAsync(gangId, ct);
            if (gangEntity is null) return null;

            var (gangResponse, gangTPHHs, slagResponse, slagTPHHs) = await GetGangAndSlagTemplateAsync(gangEntity, ct);
            var (processParams, thongKeItems) = await GetProcessAndStatisticTemplateAsync(gangEntity.ID, ct);

            return new GangTemplateConfigResponse(
                gangResponse,
                gangTPHHs,
                slagResponse,
                slagTPHHs,
                processParams,
                thongKeItems);
        }

        public async Task<GangDichConfigDetailResponse?> GetGangDichDetailWithConfigAsync(int gangId, CancellationToken ct = default)
        {
            var gangDetail = await GetDetailByIdAsync(gangId, ct);
            if (gangDetail is null)
            {
                return null;
            }

            var isGangTarget = gangDetail.Quang.Loai_Quang == (int)LoaiQuang.Gang && gangDetail.Quang.ID_Quang_Gang == null;
            if (!isGangTarget)
            {
                return null;
            }

            var slagEntity = await _db.Quang.AsNoTracking()
                .Where(x => x.Loai_Quang == 4
                            && x.ID_Quang_Gang == gangId
                            && !x.Da_Xoa)
                .OrderByDescending(x => x.Ngay_Tao)
                .FirstOrDefaultAsync(ct);

            QuangDetailResponse? slagDetail = null;
            if (slagEntity is not null)
            {
                slagDetail = await GetDetailByIdAsync(slagEntity.ID, ct);
            }

            var (processParams, thongKeItems) = await GetProcessAndStatisticTemplateAsync(gangId, ct);
            return new GangDichConfigDetailResponse(gangDetail, slagDetail, processParams, thongKeItems);
        }

        public async Task<int> UpsertKetQuaWithThanhPhanAsync(QuangKetQuaUpsertDto dto, CancellationToken ct = default)
        {
            var hasExternalTransaction = _db.Database.CurrentTransaction != null;
            IDbContextTransaction? tx = null;
            if (!hasExternalTransaction)
            {
                tx = await _db.Database.BeginTransactionAsync(ct);
            }
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
                        Nguoi_Tao = dto.Nguoi_Tao,
                        ID_Quang_Gang = dto.ID_Quang_Gang
                    };
                    _db.Quang.Add(entity);
                    await _db.SaveChangesAsync(ct);
                }
                else
                {
                    // Update existing result ore
                    var foundEntity = await _db.Quang.FindAsync(new object[] { dto.ID.Value }, ct);
                    if (foundEntity == null) throw new ArgumentException($"Quặng kết quả {dto.ID} không tồn tại");
                    entity = foundEntity;

                    entity.Ma_Quang = dto.Ma_Quang;
                    entity.Ten_Quang = dto.Ten_Quang;
                    entity.Loai_Quang = dto.Loai_Quang;
                    entity.Dang_Hoat_Dong = dto.Dang_Hoat_Dong;
                    entity.Ghi_Chu = dto.Ghi_Chu;
                    entity.ID_Quang_Gang = dto.ID_Quang_Gang;
                    // Update ID_Quang_Gang dựa trên Loai_Quang:
                    // - Gang (Loai_Quang = 2): ID_Quang_Gang = null
                    // - Xỉ (Loai_Quang = 4): ID_Quang_Gang = dto.ID_Quang_Gang (phải có giá trị)
                    if (dto.Loai_Quang == 2)
                    {
                        // Gang: luôn set null
                        entity.ID_Quang_Gang = null;
                    }
                    else if (dto.Loai_Quang == 4)
                    {
                        // Xỉ: phải có ID_Quang_Gang
                        if (dto.ID_Quang_Gang.HasValue)
                        {
                            entity.ID_Quang_Gang = dto.ID_Quang_Gang;
                        }
                        // Nếu không có giá trị, giữ nguyên giá trị cũ để tránh mất dữ liệu
                    }
                    else
                    {
                        // Các loại quặng khác: chỉ update nếu có giá trị
                        if (dto.ID_Quang_Gang.HasValue)
                        {
                            entity.ID_Quang_Gang = dto.ID_Quang_Gang;
                        }
                    }
                    entity.Ngay_Sua = DateTimeOffset.Now;
                    entity.Nguoi_Sua = dto.Nguoi_Tao;

                    _db.Quang.Update(entity);
                    await _db.SaveChangesAsync(ct);
                }

                // Upsert chemical composition using optimized logic
                await UpsertChemicalCompositionAsync(entity.ID, dto.ThanhPhanHoaHoc, ct);

                // Upsert PA_Quang_KQ mapping
                await UpsertPaQuangKqMappingAsync(dto.ID_PhuongAn, entity.ID, dto.Loai_Quang, ct);

                if (!hasExternalTransaction && tx is not null)
                {
                    await tx.CommitAsync(ct);
                }
                return entity.ID;
            }
            catch
            {
                if (!hasExternalTransaction && tx is not null)
                {
                    await tx.RollbackAsync(ct);
                }
                throw;
            }
            finally
            {
                if (!hasExternalTransaction && tx is not null)
                {
                    await tx.DisposeAsync();
                }
            }
        }

        public async Task<int> UpsertGangDichWithConfigAsync(GangDichConfigUpsertDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(dto.Gang);

            if (dto.Gang.Loai_Quang != (int)LoaiQuang.Gang)
            {
                throw new InvalidOperationException("Gang đích phải có loại quặng = 2 (Gang).");
            }

            if (dto.Gang.ID_Quang_Gang.HasValue)
            {
                throw new InvalidOperationException("Gang đích không được tham chiếu tới quặng khác (ID_Quang_Gang phải null).");
            }

            if (dto.Slag != null && dto.Slag.Loai_Quang != (int)LoaiQuang.Xi)
            {
                throw new InvalidOperationException("Cấu hình xỉ phải có loại quặng = 4 (Xỉ).");
            }

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var gangDto = dto.Gang with
                {
                    ID_Quang_Gang = null,
                    TemplateConfig = dto.TemplateConfig ?? dto.Gang.TemplateConfig,
                    SaveAsTemplate = dto.Gang.SaveAsTemplate || dto.TemplateConfig != null
                };

                var gangId = await UpsertWithThanhPhanAsync(gangDto, ct);

                if (dto.Slag != null)
                {
                    var shouldSaveSlagTemplate = dto.Slag.SaveAsTemplate ||
                                                 dto.Gang.SaveAsTemplate ||
                                                 dto.TemplateConfig != null;

                    var slagDto = dto.Slag with
                    {
                        ID_Quang_Gang = dto.Slag.ID_Quang_Gang ?? gangId,
                        SaveAsTemplate = shouldSaveSlagTemplate,
                        TemplateConfig = null
                    };

                    await UpsertWithThanhPhanAsync(slagDto, ct);
                }

                await transaction.CommitAsync(ct);
                return gangId;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
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

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _set.FirstOrDefaultAsync(x => x.ID == id, ct);
            if (entity is null) return false;

            // Kiểm tra xem Quang có đang được sử dụng trong các bảng phụ không
            var usedInCTPChiTietQuang = await _db.CTP_ChiTiet_Quang
                .AnyAsync(x => x.ID_Quang_DauVao == id && !x.Da_Xoa, ct);
            
            if (usedInCTPChiTietQuang)
            {
                throw new InvalidOperationException("Không thể xóa quặng này. Quặng đang được sử dụng trong chi tiết công thức phối.");
            }

            var usedInCTPBangChiPhi = await _db.CTP_BangChiPhi
                .AnyAsync(x => x.ID_Quang == id, ct);
            
            if (usedInCTPBangChiPhi)
            {
                throw new InvalidOperationException("Không thể xóa quặng này. Quặng đang được sử dụng trong bảng chi phí công thức phối.");
            }

            var usedInPAQuangKQ = await _db.PA_Quang_KQ
                .AnyAsync(x => x.ID_Quang == id, ct);
            
            if (usedInPAQuangKQ)
            {
                throw new InvalidOperationException("Không thể xóa quặng này. Quặng đang được sử dụng trong kết quả phương án phối.");
            }

            var usedInCongThucPhoi = await _db.Cong_Thuc_Phoi
                .AnyAsync(x => x.ID_Quang_DauRa == id && !x.Da_Xoa, ct);
            
            if (usedInCongThucPhoi)
            {
                throw new InvalidOperationException("Không thể xóa quặng này. Quặng đang được sử dụng như quặng đầu ra trong công thức phối.");
            }

            _set.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteQuangWithRelatedDataAsync(int id, ICong_Thuc_PhoiRepository congThucPhoiRepo, CancellationToken ct = default)
        {
            var entity = await _set.FirstOrDefaultAsync(x => x.ID == id, ct);
            if (entity is null) return false;

            // Bước 1: Kiểm tra xem quặng này (bất kỳ loại nào: thường, trộn, gang, xỉ...) 
            // có phải quặng đầu vào của công thức phối nào không
            // Áp dụng cho TẤT CẢ các loại quặng, bao gồm cả quặng được trộn
            var usedAsInputOre = await _db.Set<CTP_ChiTiet_Quang>()
                .Where(x => x.ID_Quang_DauVao == id && !x.Da_Xoa)
                .Join(_db.Set<Cong_Thuc_Phoi>()
                    .Where(ctp => !ctp.Da_Xoa),
                    ctq => ctq.ID_Cong_Thuc_Phoi,
                    ctp => ctp.ID,
                    (ctq, ctp) => new { FormulaId = ctp.ID, FormulaCode = ctp.Ma_Cong_Thuc })
                .FirstOrDefaultAsync(ct);

            if (usedAsInputOre != null)
            {
                throw new InvalidOperationException($"Không thể xóa quặng này. Quặng đang được sử dụng như quặng đầu vào trong công thức phối '{usedAsInputOre.FormulaCode ?? $"ID:{usedAsInputOre.FormulaId}"}'.");
            }

            // Bước 2: Nếu là quặng được trộn (loại 1, 6, hoặc 7), xóa công thức phối tạo ra nó
            // Quặng được trộn khác quặng thường ở chỗ: nó được tạo ra từ công thức phối
            // Loại 1: Trộn bình thường, Loại 6: (reserved), Loại 7: Trộn trong phương án
            // Nên khi xóa quặng được trộn, cần xóa luôn công thức phối tạo ra nó
            if (entity.Loai_Quang == 1 || entity.Loai_Quang == 6 || entity.Loai_Quang == 7)
            {
                // Tìm công thức phối có quặng này là đầu ra
                var formulasWithThisOutput = await _db.Set<Cong_Thuc_Phoi>()
                    .Where(x => x.ID_Quang_DauRa == id && !x.Da_Xoa)
                    .Select(x => x.ID)
                    .ToListAsync(ct);

                // Xóa tất cả công thức phối có quặng này là đầu ra
                foreach (var formulaId in formulasWithThisOutput)
                {
                    await congThucPhoiRepo.DeleteCongThucPhoiWithRelatedDataAsync(formulaId, ct);
                }

                // Sau khi xóa công thức phối, quặng đầu ra đã được xóa bởi DeleteCongThucPhoiWithRelatedDataAsync
                // Kiểm tra xem quặng còn tồn tại không
                var stillExists = await _set.AnyAsync(x => x.ID == id, ct);
                if (!stillExists)
                {
                    return true; // Đã được xóa bởi DeleteCongThucPhoiWithRelatedDataAsync
                }
            }

            // Bước 3: Nếu là gang (Loai_Quang = 2), kiểm tra và xóa xỉ liên kết (nếu có)
            if (entity.Loai_Quang == 2 && entity.ID_Quang_Gang == null)
            {
                // Tìm xỉ liên kết với gang này
                var linkedSlag = await _set
                    .Where(x => x.Loai_Quang == 4 && x.ID_Quang_Gang == id && !x.Da_Xoa)
                    .FirstOrDefaultAsync(ct);
                
                if (linkedSlag != null)
                {
                    // Xóa xỉ trước
                    await DeleteQuangWithRelatedDataAsync(linkedSlag.ID, congThucPhoiRepo, ct);
                }
            }

            // Bước 4: Nếu là xỉ (Loai_Quang = 4) và có ID_Quang_Gang, kiểm tra xem có gang nào đang tham chiếu không
            if (entity.Loai_Quang == 4 && entity.ID_Quang_Gang.HasValue)
            {
                var gangId = entity.ID_Quang_Gang.Value;
                var gangExists = await _set.AnyAsync(x => x.ID == gangId && !x.Da_Xoa, ct);
                if (!gangExists)
                {
                    throw new InvalidOperationException($"Không thể xóa xỉ này. Gang đích (ID: {gangId}) không tồn tại hoặc đã bị xóa.");
                }
            }

            // Xóa Quang_TP_PhanTich
            var quangTPPhanTich = await _db.Quang_TP_PhanTich
                .Where(x => x.ID_Quang == id)
                .ToListAsync(ct);
            if (quangTPPhanTich.Any())
            {
                _db.Quang_TP_PhanTich.RemoveRange(quangTPPhanTich);
            }

            // Xóa Quang_Gia_LichSu
            var quangGiaLichSu = await _db.Quang_Gia_LichSu
                .Where(x => x.ID_Quang == id)
                .ToListAsync(ct);
            if (quangGiaLichSu.Any())
            {
                _db.Quang_Gia_LichSu.RemoveRange(quangGiaLichSu);
            }

            // Xóa Quang
            _set.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteGangDichWithRelatedDataAsync(int gangDichId, IPhuong_An_PhoiRepository phuongAnRepo, ICong_Thuc_PhoiRepository congThucRepo, CancellationToken ct = default)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // 1. Kiểm tra xem quặng có phải là gang đích không
                var gangDichEntity = await _set.FirstOrDefaultAsync(x => x.ID == gangDichId, ct);
                if (gangDichEntity == null) return false;

                if (gangDichEntity.Loai_Quang != 2 || gangDichEntity.ID_Quang_Gang != null)
                {
                    // Không phải gang đích
                    await tx.RollbackAsync(ct);
                    return false;
                }

                // 2. Lấy danh sách phương án có ID_Quang_Dich = gang đích ID
                var plans = await phuongAnRepo.GetByQuangDichAsync(gangDichId, ct);

                // 3. Xóa từng phương án (gọi hàm xóa phương án - Cấp 3)
                foreach (var plan in plans)
                {
                    await phuongAnRepo.DeletePlanWithRelatedDataAsync(plan.ID, congThucRepo, this, ct);
                }

                // 4. Xóa template config
                var templateConfigs = await _db.Gang_Dich_Template_Config
                    .Where(x => x.ID_Gang_Dich == gangDichId)
                    .ToListAsync(ct);
                if (templateConfigs.Any())
                {
                    _db.Gang_Dich_Template_Config.RemoveRange(templateConfigs);
                    await _db.SaveChangesAsync(ct);
                }

                // 5. Xóa xỉ liên quan (nếu có)
                var slagEntity = await _db.Quang
                    .Where(x => x.Loai_Quang == 4
                                && x.ID_Quang_Gang == gangDichId
                                && !x.Da_Xoa)
                    .FirstOrDefaultAsync(ct);
                if (slagEntity != null)
                {
                    await DeleteQuangWithRelatedDataAsync(slagEntity.ID, congThucRepo, ct);
                }

                // 6. Xóa quặng gang đích (gọi hàm xóa quặng - Cấp 1)
                await DeleteQuangWithRelatedDataAsync(gangDichId, congThucRepo, ct);

                await tx.CommitAsync(ct);
                return true;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private async Task UpdateTemplateConfigAsync(int gangDichId, GangTemplateConfigDto? config, int? nguoiTao, CancellationToken ct)
        {
            var existingConfigs = await _db.Gang_Dich_Template_Config
                .Where(x => x.ID_Gang_Dich == gangDichId)
                .ToListAsync(ct);

            if (existingConfigs.Any())
            {
                _db.Gang_Dich_Template_Config.RemoveRange(existingConfigs);
            }

            if (config == null)
            {
                return;
            }

            var now = DateTime.Now;
            var toAdd = new List<Gang_Dich_Template_Config>();

            if (config.ProcessParams != null)
            {
                foreach (var item in config.ProcessParams)
                {
                    toAdd.Add(new Gang_Dich_Template_Config
                    {
                        ID_Gang_Dich = gangDichId,
                        Loai_Template = 1,
                        ID_Reference = item.Id,
                        ThuTu = item.ThuTu,
                        Ngay_Tao = now,
                        Nguoi_Tao = nguoiTao,
                        Da_Xoa = false
                    });
                }
            }

            if (config.ThongKes != null)
            {
                foreach (var item in config.ThongKes)
                {
                    toAdd.Add(new Gang_Dich_Template_Config
                    {
                        ID_Gang_Dich = gangDichId,
                        Loai_Template = 2,
                        ID_Reference = item.Id,
                        ThuTu = item.ThuTu,
                        Ngay_Tao = now,
                        Nguoi_Tao = nguoiTao,
                        Da_Xoa = false
                    });
                }
            }

            if (toAdd.Count > 0)
            {
                await _db.Gang_Dich_Template_Config.AddRangeAsync(toAdd, ct);
            }
        }

        private static QuangResponse MapToQuangResponse(Quang entity)
        {
            return new QuangResponse(
                entity.ID,
                entity.Ma_Quang,
                entity.Ten_Quang ?? string.Empty,
                entity.Loai_Quang,
                entity.Dang_Hoat_Dong,
                entity.Da_Xoa,
                entity.Ghi_Chu,
                entity.Ngay_Tao,
                entity.Nguoi_Tao,
                entity.Ngay_Sua,
                entity.Nguoi_Sua,
                null,
                null,
                null,
                null,
                null,
                entity.ID_Quang_Gang
            );
        }

        private async Task<Quang?> FindGangTemplateEntityAsync(int? gangId, CancellationToken ct)
        {
            var gangQuery = _set.AsNoTracking()
                .Where(x => x.Loai_Quang == 2 && x.ID_Quang_Gang == null && !x.Da_Xoa);

            if (gangId.HasValue && gangId.Value > 0)
            {
                return await gangQuery.FirstOrDefaultAsync(x => x.ID == gangId.Value, ct);
            }

            return await gangQuery
                .OrderByDescending(x => x.Is_Template == true)
                .ThenByDescending(x => x.Ngay_Tao)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<(QuangResponse Gang, IReadOnlyList<TPHHOfQuangResponse> GangTPHHs, QuangResponse? Slag, IReadOnlyList<TPHHOfQuangResponse> SlagTPHHs)>
            GetGangAndSlagTemplateAsync(Quang gangEntity, CancellationToken ct)
        {
            var gangResponse = MapToQuangResponse(gangEntity);
            var gangTPHHs = await LoadChemistryTemplateAsync(gangEntity.ID, ct);

            var slagEntity = await _db.Quang.AsNoTracking()
                .Where(x => x.Loai_Quang == 4
                            && x.ID_Quang_Gang == gangEntity.ID
                            && !x.Da_Xoa
                            && x.Is_Template == true)
                .OrderByDescending(x => x.Ngay_Tao)
                .FirstOrDefaultAsync(ct);

            QuangResponse? slagResponse = null;
            IReadOnlyList<TPHHOfQuangResponse> slagTPHHs = Array.Empty<TPHHOfQuangResponse>();

            if (slagEntity is not null)
            {
                slagResponse = MapToQuangResponse(slagEntity);
                slagTPHHs = await LoadChemistryTemplateAsync(slagEntity.ID, ct);
            }

            return (gangResponse, gangTPHHs, slagResponse, slagTPHHs);
        }

        private async Task<(IReadOnlyList<ProcessParamTemplateItem> ProcessParams, IReadOnlyList<ThongKeTemplateItem> ThongKes)>
            GetProcessAndStatisticTemplateAsync(int gangId, CancellationToken ct)
        {
            var processParams = await _db.Gang_Dich_Template_Config.AsNoTracking()
                .Where(x => x.ID_Gang_Dich == gangId
                            && x.Loai_Template == 1
                            && !x.Da_Xoa)
                .Join(_db.LoCao_ProcessParam.AsNoTracking()
                        .Where(p => p.Da_Xoa == null || p.Da_Xoa == false),
                    template => template.ID_Reference,
                    param => param.ID,
                    (template, param) => new
                    {
                        param.ID,
                        param.Code,
                        param.Ten,
                        param.DonVi,
                        template.ThuTu
                    })
                .OrderBy(x => x.ThuTu)
                .Select(x => new ProcessParamTemplateItem(
                    x.ID,
                    x.Code,
                    x.Ten,
                    x.DonVi,
                    x.ThuTu))
                .ToListAsync(ct);

            var thongKeItems = await _db.Gang_Dich_Template_Config.AsNoTracking()
                .Where(x => x.ID_Gang_Dich == gangId
                            && x.Loai_Template == 2
                            && !x.Da_Xoa)
                .Join(_db.ThongKe_Function.AsNoTracking()
                        .Where(f => f.IsActive),
                    template => template.ID_Reference,
                    func => func.ID,
                    (template, func) => new
                    {
                        func.ID,
                        func.Code,
                        func.Ten,
                        func.DonVi,
                        template.ThuTu
                    })
                .OrderBy(x => x.ThuTu)
                .Select(x => new ThongKeTemplateItem(
                    x.ID,
                    x.Code,
                    x.Ten,
                    x.DonVi,
                    x.ThuTu))
                .ToListAsync(ct);

            return (processParams, thongKeItems);
        }

        private async Task<List<TPHHOfQuangResponse>> LoadChemistryTemplateAsync(int quangId, CancellationToken ct)
        {
            async Task<List<TPHHOfQuangResponse>> QueryAsync(bool templateOnly, CancellationToken token)
            {
                var baseQuery = _db.Quang_TP_PhanTich.AsNoTracking()
                    .Where(x => x.ID_Quang == quangId && !x.Da_Xoa);

                if (templateOnly)
                {
                    baseQuery = baseQuery.Where(x => x.Is_Template == true);
                }

                return await baseQuery
                    .Join(_db.TP_HoaHoc.AsNoTracking(),
                        qt => qt.ID_TPHH,
                        tphh => tphh.ID,
                        (qt, tphh) => new { qt, tphh })
                    .OrderBy(x => x.qt.ThuTuTPHH ?? int.MaxValue)
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
                    .ToListAsync(token);
            }

            var templateData = await QueryAsync(true, ct);
            if (templateData.Count > 0)
            {
                return templateData;
            }

            return await QueryAsync(false, ct);
        }
    }
}

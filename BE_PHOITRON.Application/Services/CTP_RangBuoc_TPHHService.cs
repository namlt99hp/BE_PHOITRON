using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class CTP_RangBuoc_TPHHService : ICTP_RangBuoc_TPHHService
    {
        private readonly ICTP_RangBuoc_TPHHRepository _ctpRangBuocRepo;
        private readonly IUnitOfWork _uow;

        public CTP_RangBuoc_TPHHService(ICTP_RangBuoc_TPHHRepository ctpRangBuocRepo, IUnitOfWork uow)
        {
            _ctpRangBuocRepo = ctpRangBuocRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<CTP_RangBuoc_TPHHResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _ctpRangBuocRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<CTP_RangBuoc_TPHHResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctpRangBuocRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(CTP_RangBuoc_TPHHCreateDto dto, CancellationToken ct = default)
        {
            var entity = new CTP_RangBuoc_TPHH
            {
                ID_Cong_Thuc_Phoi = dto.ID_Cong_Thuc_Phoi,
                ID_TPHH = dto.ID_TPHH,
                Min_PhanTram = dto.Min_PhanTram,
                Max_PhanTram = dto.Max_PhanTram,
                Rang_Buoc_Cung = dto.Rang_Buoc_Cung,
                Uu_Tien = dto.Uu_Tien,
                Ghi_Chu = dto.Ghi_Chu
            };

            await _ctpRangBuocRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(CTP_RangBuoc_TPHHUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _ctpRangBuocRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            entity.ID_Cong_Thuc_Phoi = dto.ID_Cong_Thuc_Phoi;
            entity.ID_TPHH = dto.ID_TPHH;
            entity.Min_PhanTram = dto.Min_PhanTram;
            entity.Max_PhanTram = dto.Max_PhanTram;
            entity.Rang_Buoc_Cung = dto.Rang_Buoc_Cung;
            entity.Uu_Tien = dto.Uu_Tien;
            entity.Ghi_Chu = dto.Ghi_Chu;

            _ctpRangBuocRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(CTP_RangBuoc_TPHHUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.CTP_RangBuoc_TPHH, ct);
            }
            else
            {
                var updateDto = new CTP_RangBuoc_TPHHUpdateDto(
                    dto.ID.Value,
                    dto.CTP_RangBuoc_TPHH.ID_Cong_Thuc_Phoi,
                    dto.CTP_RangBuoc_TPHH.ID_TPHH,
                    dto.CTP_RangBuoc_TPHH.Min_PhanTram,
                    dto.CTP_RangBuoc_TPHH.Max_PhanTram,
                    dto.CTP_RangBuoc_TPHH.Rang_Buoc_Cung,
                    dto.CTP_RangBuoc_TPHH.Uu_Tien,
                    dto.CTP_RangBuoc_TPHH.Ghi_Chu
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctpRangBuocRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _ctpRangBuocRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IReadOnlyList<CTP_RangBuoc_TPHHResponse>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default)
        {
            var entities = await _ctpRangBuocRepo.GetByCongThucPhoiAsync(idCongThucPhoi, ct);
            return entities.Select(MapToResponse).ToList();
        }

        private static CTP_RangBuoc_TPHHResponse MapToResponse(CTP_RangBuoc_TPHH entity) => new(
            entity.ID,
            entity.ID_Cong_Thuc_Phoi,
            entity.ID_TPHH,
            entity.Min_PhanTram,
            entity.Max_PhanTram,
            entity.Rang_Buoc_Cung,
            entity.Uu_Tien,
            entity.Ghi_Chu,
            // Navigation properties - will be populated by repository if needed
            null, // Cong_Thuc_Phoi_Ma
            null, // Cong_Thuc_Phoi_Ten
            null, // TP_HoaHoc_Ma
            null  // TP_HoaHoc_Ten
        );
    }
}

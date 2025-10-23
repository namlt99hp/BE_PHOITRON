using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class Cong_Thuc_PhoiService : ICong_Thuc_PhoiService
    {
        private readonly ICong_Thuc_PhoiRepository _congThucPhoiRepo;
        private readonly ICTP_ChiTiet_QuangRepository _ctpChiTietQuangRepo;
        private readonly IUnitOfWork _uow;

        public Cong_Thuc_PhoiService(
            ICong_Thuc_PhoiRepository congThucPhoiRepo,
            ICTP_ChiTiet_QuangRepository ctpChiTietQuangRepo,
            IUnitOfWork uow)
        {
            _congThucPhoiRepo = congThucPhoiRepo;
            _ctpChiTietQuangRepo = ctpChiTietQuangRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<Cong_Thuc_PhoiResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _congThucPhoiRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<Cong_Thuc_PhoiResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _congThucPhoiRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(Cong_Thuc_PhoiCreateDto dto, CancellationToken ct = default)
        {
            // Validate business rules
            if (await _congThucPhoiRepo.ExistsByCodeAsync(dto.Ma_Cong_Thuc, ct))
                throw new InvalidOperationException($"Mã công thức phối '{dto.Ma_Cong_Thuc}' đã tồn tại.");

            if (await _congThucPhoiRepo.HasOverlappingPeriodAsync(dto.ID_Quang_DauRa, dto.Hieu_Luc_Tu, dto.Hieu_Luc_Den, null, ct))
                throw new InvalidOperationException($"Công thức phối cho quặng đầu ra này đã có hiệu lực trong khoảng thời gian này.");

            var entity = new Cong_Thuc_Phoi
            {
                ID_Quang_DauRa = dto.ID_Quang_DauRa,
                Ma_Cong_Thuc = dto.Ma_Cong_Thuc,
                Ten_Cong_Thuc = dto.Ten_Cong_Thuc,
                He_So_Thu_Hoi = dto.He_So_Thu_Hoi,
                Chi_Phi_Cong_Doạn_1Tan = dto.Chi_Phi_Cong_Doạn_1Tan,
                Phien_Ban = dto.Phien_Ban,
                Trang_Thai = dto.Trang_Thai,
                Hieu_Luc_Tu = dto.Hieu_Luc_Tu,
                Hieu_Luc_Den = dto.Hieu_Luc_Den,
                Ghi_Chu = dto.Ghi_Chu,
                Ngay_Tao = DateTimeOffset.Now,
                Nguoi_Tao = null // TODO: Get from current user context
            };

            await _congThucPhoiRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(Cong_Thuc_PhoiUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _congThucPhoiRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            // Validate business rules
            if (await _congThucPhoiRepo.ExistsByCodeAsync(dto.Ma_Cong_Thuc, ct))
            {
                var existingEntity = await _congThucPhoiRepo.FindAsync(x => x.Ma_Cong_Thuc == dto.Ma_Cong_Thuc, false, ct);
                if (existingEntity.FirstOrDefault()?.ID != dto.ID)
                    throw new InvalidOperationException($"Mã công thức phối '{dto.Ma_Cong_Thuc}' đã tồn tại.");
            }

            if (await _congThucPhoiRepo.HasOverlappingPeriodAsync(dto.ID_Quang_DauRa, dto.Hieu_Luc_Tu, dto.Hieu_Luc_Den, dto.ID, ct))
                throw new InvalidOperationException($"Công thức phối cho quặng đầu ra này đã có hiệu lực trong khoảng thời gian này.");

            entity.ID_Quang_DauRa = dto.ID_Quang_DauRa;
            entity.Ma_Cong_Thuc = dto.Ma_Cong_Thuc;
            entity.Ten_Cong_Thuc = dto.Ten_Cong_Thuc;
            entity.He_So_Thu_Hoi = dto.He_So_Thu_Hoi;
            entity.Chi_Phi_Cong_Doạn_1Tan = dto.Chi_Phi_Cong_Doạn_1Tan;
            entity.Phien_Ban = dto.Phien_Ban;
            entity.Trang_Thai = dto.Trang_Thai;
            entity.Hieu_Luc_Tu = dto.Hieu_Luc_Tu;
            entity.Hieu_Luc_Den = dto.Hieu_Luc_Den;
            entity.Ghi_Chu = dto.Ghi_Chu;
            entity.Ngay_Sua = DateTimeOffset.Now;
            entity.Nguoi_Sua = null; // TODO: Get from current user context

            _congThucPhoiRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(Cong_Thuc_PhoiUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.Cong_Thuc_Phoi, ct);
            }
            else
            {
                var updateDto = new Cong_Thuc_PhoiUpdateDto(
                    dto.ID.Value,
                    dto.Cong_Thuc_Phoi.ID_Quang_DauRa,
                    dto.Cong_Thuc_Phoi.Ma_Cong_Thuc,
                    dto.Cong_Thuc_Phoi.Hieu_Luc_Tu,
                    dto.Cong_Thuc_Phoi.Ten_Cong_Thuc,
                    dto.Cong_Thuc_Phoi.He_So_Thu_Hoi,
                    dto.Cong_Thuc_Phoi.Chi_Phi_Cong_Doạn_1Tan,
                    dto.Cong_Thuc_Phoi.Phien_Ban,
                    dto.Cong_Thuc_Phoi.Trang_Thai,
                    dto.Cong_Thuc_Phoi.Hieu_Luc_Den,
                    dto.Cong_Thuc_Phoi.Ghi_Chu
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _congThucPhoiRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _congThucPhoiRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ExistsByCodeAsync(string maCongThuc, CancellationToken ct = default)
            => await _congThucPhoiRepo.ExistsByCodeAsync(maCongThuc, ct);

        public async Task<IReadOnlyList<Cong_Thuc_PhoiResponse>> GetByQuangDauRaAsync(int idQuangDauRa, CancellationToken ct = default)
        {
            var entities = await _congThucPhoiRepo.GetByQuangDauRaAsync(idQuangDauRa, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<IReadOnlyList<Cong_Thuc_PhoiResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _congThucPhoiRepo.GetActiveAsync(ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> HasOverlappingPeriodAsync(int idQuangDauRa, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default)
            => await _congThucPhoiRepo.HasOverlappingPeriodAsync(idQuangDauRa, hieuLucTu, hieuLucDen, excludeId, ct);

        public async Task<bool> ValidateTotalPercentageAsync(int idCongThucPhoi, CancellationToken ct = default)
            => await _ctpChiTietQuangRepo.ValidateTotalPercentageAsync(idCongThucPhoi, ct);

        public async Task<bool> DeleteCongThucPhoiAsync(int id, CancellationToken ct = default)
            => await _congThucPhoiRepo.DeleteCongThucPhoiAsync(id, ct);

        private static Cong_Thuc_PhoiResponse MapToResponse(Cong_Thuc_Phoi entity) => new(
            entity.ID,
            entity.ID_Quang_DauRa,
            entity.Ma_Cong_Thuc,
            entity.Ten_Cong_Thuc,
            entity.He_So_Thu_Hoi,
            entity.Chi_Phi_Cong_Doạn_1Tan,
            entity.Phien_Ban,
            entity.Trang_Thai,
            entity.Hieu_Luc_Tu,
            entity.Hieu_Luc_Den,
            entity.Ghi_Chu,
            entity.Ngay_Tao,
            entity.Nguoi_Tao,
            entity.Ngay_Sua,
            entity.Nguoi_Sua,
            // Navigation properties - will be populated by repository if needed
            null, // Quang_DauRa_Ma
            null, // Quang_DauRa_Ten
            // Calculated properties - will be calculated by repository if needed
            null, // Tong_Ti_Le_Phan_Tram
            null  // So_Luong_Quang_DauVao
        );
    }
}

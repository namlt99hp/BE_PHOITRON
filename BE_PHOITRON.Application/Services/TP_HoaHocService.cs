using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class TP_HoaHocService : ITP_HoaHocService
    {
        private readonly ITP_HoaHocRepository _tpHHRepo;
        private readonly IUnitOfWork _uow;

        public TP_HoaHocService(ITP_HoaHocRepository tpHHRepo, IUnitOfWork uow)
        {
            _tpHHRepo = tpHHRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<TP_HoaHocResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _tpHHRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<TP_HoaHocResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _tpHHRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(TP_HoaHocCreateDto dto, CancellationToken ct = default)
        {
            // Validate business rules
            if (await _tpHHRepo.ExistsByCodeAsync(dto.Ma_TPHH, ct))
                throw new InvalidOperationException($"Mã thành phần hóa học '{dto.Ma_TPHH}' đã tồn tại.");

            var entity = new TP_HoaHoc
            {
                Ma_TPHH = dto.Ma_TPHH,
                Ten_TPHH = dto.Ten_TPHH,
                Don_Vi = dto.Don_Vi,
                Thu_Tu = dto.Thu_Tu,
                Ghi_Chu = dto.Ghi_Chu,
                Da_Xoa = false
            };

            await _tpHHRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(TP_HoaHocUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _tpHHRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            // Validate business rules
            if (await _tpHHRepo.ExistsByCodeAsync(dto.Ma_TPHH, ct))
            {
                var existingEntity = await _tpHHRepo.FindAsync(x => x.Ma_TPHH == dto.Ma_TPHH, false, ct);
                if (existingEntity.FirstOrDefault()?.ID != dto.ID)
                    throw new InvalidOperationException($"Mã thành phần hóa học '{dto.Ma_TPHH}' đã tồn tại.");
            }

            entity.Ma_TPHH = dto.Ma_TPHH;
            entity.Ten_TPHH = dto.Ten_TPHH;
            entity.Don_Vi = dto.Don_Vi;
            entity.Thu_Tu = dto.Thu_Tu;
            entity.Ghi_Chu = dto.Ghi_Chu;

            _tpHHRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(TP_HoaHocUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.TP_HoaHoc, ct);
            }
            else
            {
                var updateDto = new TP_HoaHocUpdateDto(
                    dto.ID.Value,
                    dto.TP_HoaHoc.Ma_TPHH,
                    dto.TP_HoaHoc.Ten_TPHH,
                    dto.TP_HoaHoc.Don_Vi,
                    dto.TP_HoaHoc.Thu_Tu,
                    dto.TP_HoaHoc.Ghi_Chu
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _tpHHRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _tpHHRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ExistsByCodeAsync(string maTPHH, CancellationToken ct = default)
            => await _tpHHRepo.ExistsByCodeAsync(maTPHH, ct);

        public async Task<IReadOnlyList<TP_HoaHocResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _tpHHRepo.GetActiveAsync(ct);
            return entities.Select(MapToResponse).ToList();
        }

        private static TP_HoaHocResponse MapToResponse(TP_HoaHoc entity) => new(
            entity.ID,
            entity.Ma_TPHH,
            entity.Ten_TPHH,
            entity.Don_Vi,
            entity.Thu_Tu,
            entity.Ghi_Chu,
            entity.Ngay_Tao,
            entity.Da_Xoa
        );
    }
}

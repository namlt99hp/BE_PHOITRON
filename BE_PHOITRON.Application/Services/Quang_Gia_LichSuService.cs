using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class Quang_Gia_LichSuService : IQuang_Gia_LichSuService
    {
        private readonly IQuang_Gia_LichSuRepository _quangGiaRepo;
        private readonly IUnitOfWork _uow;

        public Quang_Gia_LichSuService(IQuang_Gia_LichSuRepository quangGiaRepo, IUnitOfWork uow)
        {
            _quangGiaRepo = quangGiaRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<Quang_Gia_LichSuResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _quangGiaRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<Quang_Gia_LichSuResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quangGiaRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(Quang_Gia_LichSuCreateDto dto, CancellationToken ct = default)
        {
            // Validate business rules
            if (await _quangGiaRepo.HasOverlappingPeriodAsync(dto.ID_Quang, dto.Hieu_Luc_Tu, dto.Hieu_Luc_Den, null, ct))
                throw new InvalidOperationException($"Đã tồn tại giá cho quặng này trong khoảng thời gian này.");

            var entity = new Quang_Gia_LichSu
            {
                ID_Quang = dto.ID_Quang,
                Don_Gia_USD_1Tan = dto.Don_Gia_USD_1Tan,
                Don_Gia_VND_1Tan = dto.Don_Gia_VND_1Tan,
                Ty_Gia_USD_VND = dto.Ty_Gia_USD_VND,
                Tien_Te = dto.Tien_Te,
                Hieu_Luc_Tu = dto.Hieu_Luc_Tu,
                Hieu_Luc_Den = dto.Hieu_Luc_Den,
                Ghi_Chu = dto.Ghi_Chu
            };

            await _quangGiaRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(Quang_Gia_LichSuUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _quangGiaRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            // Validate business rules
            if (await _quangGiaRepo.HasOverlappingPeriodAsync(dto.ID_Quang, dto.Hieu_Luc_Tu, dto.Hieu_Luc_Den, dto.ID, ct))
                throw new InvalidOperationException($"Đã tồn tại giá cho quặng này trong khoảng thời gian này.");

            entity.ID_Quang = dto.ID_Quang;
            entity.Don_Gia_USD_1Tan = dto.Don_Gia_USD_1Tan;
            entity.Don_Gia_VND_1Tan = dto.Don_Gia_VND_1Tan;
            entity.Ty_Gia_USD_VND = dto.Ty_Gia_USD_VND;
            entity.Tien_Te = dto.Tien_Te;
            entity.Hieu_Luc_Tu = dto.Hieu_Luc_Tu;
            entity.Hieu_Luc_Den = dto.Hieu_Luc_Den;
            entity.Ghi_Chu = dto.Ghi_Chu;

            _quangGiaRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(Quang_Gia_LichSuUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.Quang_Gia_LichSu, ct);
            }
            else
            {
                var updateDto = new Quang_Gia_LichSuUpdateDto(
                    dto.ID.Value,
                    dto.Quang_Gia_LichSu.ID_Quang,
                    dto.Quang_Gia_LichSu.Don_Gia_USD_1Tan,
                    dto.Quang_Gia_LichSu.Don_Gia_VND_1Tan,
                    dto.Quang_Gia_LichSu.Ty_Gia_USD_VND,
                    dto.Quang_Gia_LichSu.Hieu_Luc_Tu,
                    dto.Quang_Gia_LichSu.Tien_Te,
                    dto.Quang_Gia_LichSu.Hieu_Luc_Den,
                    dto.Quang_Gia_LichSu.Ghi_Chu
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quangGiaRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _quangGiaRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IReadOnlyList<Quang_Gia_LichSuResponse>> GetByQuangAsync(int idQuang, CancellationToken ct = default)
        {
            var entities = await _quangGiaRepo.GetByQuangAsync(idQuang, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<Quang_Gia_LichSuResponse?> GetCurrentPriceAsync(int idQuang, DateTimeOffset ngayTinhToan, CancellationToken ct = default)
        {
            var entity = await _quangGiaRepo.GetCurrentPriceAsync(idQuang, ngayTinhToan, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<IReadOnlyList<Quang_Gia_LichSuResponse>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default)
        {
            var entities = await _quangGiaRepo.GetByQuangAndDateAsync(idQuang, ngayTinh, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> HasOverlappingPeriodAsync(int idQuang, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default)
        {
            return await _quangGiaRepo.HasOverlappingPeriodAsync(idQuang, hieuLucTu, hieuLucDen, excludeId, ct);
        }

        public async Task<decimal?> GetGiaByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default)
        {
            var entity = await _quangGiaRepo.GetCurrentPriceAsync(idQuang, ngayTinh, ct);
            return entity?.Don_Gia_VND_1Tan; // Return VND price as primary
        }

        private static Quang_Gia_LichSuResponse MapToResponse(Quang_Gia_LichSu entity) => new(
            entity.ID,
            entity.ID_Quang,
            entity.Don_Gia_USD_1Tan,
            entity.Don_Gia_VND_1Tan,
            entity.Ty_Gia_USD_VND,
            entity.Tien_Te,
            entity.Hieu_Luc_Tu,
            entity.Hieu_Luc_Den,
            entity.Ghi_Chu,
            // No navigation usage per design
            null,
            null
        );
    }
}
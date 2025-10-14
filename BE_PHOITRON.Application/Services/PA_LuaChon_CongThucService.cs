using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class PA_LuaChon_CongThucService : IPA_LuaChon_CongThucService
    {
        private readonly IPA_LuaChon_CongThucRepository _paLuaChonRepo;
        private readonly IUnitOfWork _uow;

        public PA_LuaChon_CongThucService(IPA_LuaChon_CongThucRepository paLuaChonRepo, IUnitOfWork uow)
        {
            _paLuaChonRepo = paLuaChonRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<PA_LuaChon_CongThucResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _paLuaChonRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<PA_LuaChon_CongThucResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _paLuaChonRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(PA_LuaChon_CongThucCreateDto dto, CancellationToken ct = default)
        {
            var entity = new PA_LuaChon_CongThuc
            {
                ID_Phuong_An = dto.ID_Phuong_An,
                ID_Quang_DauRa = dto.ID_Quang_DauRa,
                ID_Cong_Thuc_Phoi = dto.ID_Cong_Thuc_Phoi,
                Milestone = dto.Milestone
            };

            await _paLuaChonRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(PA_LuaChon_CongThucUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _paLuaChonRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            entity.ID_Phuong_An = dto.ID_Phuong_An;
            entity.ID_Quang_DauRa = dto.ID_Quang_DauRa;
            entity.ID_Cong_Thuc_Phoi = dto.ID_Cong_Thuc_Phoi;
            entity.Milestone = dto.Milestone;

            _paLuaChonRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(PA_LuaChon_CongThucUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.PA_LuaChon_CongThuc, ct);
            }
            else
            {
                var updateDto = new PA_LuaChon_CongThucUpdateDto(
                    dto.ID.Value,
                    dto.PA_LuaChon_CongThuc.ID_Phuong_An,
                    dto.PA_LuaChon_CongThuc.ID_Quang_DauRa,
                    dto.PA_LuaChon_CongThuc.ID_Cong_Thuc_Phoi,
                    dto.PA_LuaChon_CongThuc.Milestone
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _paLuaChonRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _paLuaChonRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IReadOnlyList<PA_LuaChon_CongThucResponse>> GetByPhuongAnAsync(int idPhuongAn, CancellationToken ct = default)
        {
            var entities = await _paLuaChonRepo.GetByPhuongAnAsync(idPhuongAn, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default)
            => await _paLuaChonRepo.ValidateNoCircularDependencyAsync(idPhuongAn, ct);

        public async Task<(int total, IReadOnlyList<PA_LuaChon_CongThucResponse> data)> SearchPagedAdvancedAsync(
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
            var (total, entities) = await _paLuaChonRepo.SearchPagedAdvancedAsync(page, pageSize, idPhuongAn, idQuangDauRa, idCongThucPhoi, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        private static PA_LuaChon_CongThucResponse MapToResponse(PA_LuaChon_CongThuc entity) => new(
            entity.ID,
            entity.ID_Phuong_An,
            entity.ID_Quang_DauRa,
            entity.ID_Cong_Thuc_Phoi,
            entity.Milestone,
            // Navigation properties - will be populated by repository if needed
            null, // Phuong_An_Ten
            null, // Quang_DauRa_Ma
            null, // Quang_DauRa_Ten
            null, // Cong_Thuc_Phoi_Ma
            null  // Cong_Thuc_Phoi_Ten
        );
    }
}

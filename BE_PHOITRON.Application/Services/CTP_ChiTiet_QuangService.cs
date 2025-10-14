using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class CTP_ChiTiet_QuangService : ICTP_ChiTiet_QuangService
    {
        private readonly ICTP_ChiTiet_QuangRepository _ctpChiTietRepo;
        private readonly IUnitOfWork _uow;

        public CTP_ChiTiet_QuangService(ICTP_ChiTiet_QuangRepository ctpChiTietRepo, IUnitOfWork uow)
        {
            _ctpChiTietRepo = ctpChiTietRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<CTP_ChiTiet_QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _ctpChiTietRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<CTP_ChiTiet_QuangResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctpChiTietRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(CTP_ChiTiet_QuangCreateDto dto, CancellationToken ct = default)
        {
            var entity = new CTP_ChiTiet_Quang
            {
                ID_Cong_Thuc_Phoi = dto.ID_Cong_Thuc_Phoi,
                ID_Quang_DauVao = dto.ID_Quang_DauVao,
                Ti_Le_Phan_Tram = dto.Ti_Le_Phan_Tram,
                Khau_Hao = dto.He_So_Hao_Hut_DauVao,
                Thu_Tu = dto.Thu_Tu,
                Ghi_Chu = dto.Ghi_Chu
            };

            await _ctpChiTietRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(CTP_ChiTiet_QuangUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _ctpChiTietRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            entity.ID_Cong_Thuc_Phoi = dto.ID_Cong_Thuc_Phoi;
            entity.ID_Quang_DauVao = dto.ID_Quang_DauVao;
            entity.Ti_Le_Phan_Tram = dto.Ti_Le_Phan_Tram;
            entity.Khau_Hao = dto.He_So_Hao_Hut_DauVao;
            entity.Thu_Tu = dto.Thu_Tu;
            entity.Ghi_Chu = dto.Ghi_Chu;

            _ctpChiTietRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(CTP_ChiTiet_QuangUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.CTP_ChiTiet_Quang, ct);
            }
            else
            {
                var updateDto = new CTP_ChiTiet_QuangUpdateDto(
                    dto.ID.Value,
                    dto.CTP_ChiTiet_Quang.ID_Cong_Thuc_Phoi,
                    dto.CTP_ChiTiet_Quang.ID_Quang_DauVao,
                    dto.CTP_ChiTiet_Quang.Ti_Le_Phan_Tram,
                    dto.CTP_ChiTiet_Quang.He_So_Hao_Hut_DauVao,
                    dto.CTP_ChiTiet_Quang.Thu_Tu,
                    dto.CTP_ChiTiet_Quang.Ghi_Chu
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctpChiTietRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _ctpChiTietRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IReadOnlyList<CTP_ChiTiet_QuangResponse>> GetByCongThucPhoiAsync(int idCongThucPhoi, CancellationToken ct = default)
        {
            var entities = await _ctpChiTietRepo.GetByCongThucPhoiAsync(idCongThucPhoi, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> ValidateTotalPercentageAsync(int idCongThucPhoi, CancellationToken ct = default)
            => await _ctpChiTietRepo.ValidateTotalPercentageAsync(idCongThucPhoi, ct);

        private static CTP_ChiTiet_QuangResponse MapToResponse(CTP_ChiTiet_Quang entity) => new(
            entity.ID,
            entity.ID_Cong_Thuc_Phoi,
            entity.ID_Quang_DauVao,
            entity.Ti_Le_Phan_Tram,
            entity.Khau_Hao,
            entity.Thu_Tu,
            entity.Ghi_Chu,
            // Navigation properties - will be populated by repository if needed
            null, // Cong_Thuc_Phoi_Ma
            null, // Cong_Thuc_Phoi_Ten
            null, // Quang_DauVao_Ma
            null  // Quang_DauVao_Ten
        );
    }
}

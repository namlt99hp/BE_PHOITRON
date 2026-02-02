using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;


namespace BE_PHOITRON.Application.Services
{
    public class QuangService : IQuangService
    {
        private readonly IQuangRepository _quangRepo;
        private readonly IPhuong_An_PhoiRepository _phuongAnRepo;
        private readonly ICong_Thuc_PhoiRepository _congThucRepo;
        private readonly IUnitOfWork _uow;

        public QuangService(
            IQuangRepository quangRepo,
            IPhuong_An_PhoiRepository phuongAnRepo,
            ICong_Thuc_PhoiRepository congThucRepo,
            IUnitOfWork uow)
        {
            _quangRepo = quangRepo;
            _phuongAnRepo = phuongAnRepo;
            _congThucRepo = congThucRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<QuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, int[]? loaiQuang = null, bool? isGangTarget = null, CancellationToken ct = default)
        {
            var (total, data) = await _quangRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, loaiQuang, isGangTarget, ct);
            return (total, data);
        }

        public async Task<QuangResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quangRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<QuangDetailResponse?> GetDetailByIdAsync(int id, CancellationToken ct = default)
        {
            return await _quangRepo.GetDetailByIdAsync(id, ct);
        }

        public async Task<int> CreateAsync(QuangCreateDto dto, CancellationToken ct = default)
        {
            // Validate business rules
            if (await _quangRepo.ExistsByCodeAsync(dto.Ma_Quang, ct))
                throw new InvalidOperationException($"Mã quặng '{dto.Ma_Quang}' đã tồn tại.");

            var entity = new Quang
            {
                Ma_Quang = dto.Ma_Quang,
                Ten_Quang = dto.Ten_Quang,
                Loai_Quang = dto.Loai_Quang,
                Dang_Hoat_Dong = dto.Dang_Hoat_Dong,
                Da_Xoa = false,
                Ghi_Chu = dto.Ghi_Chu,
                Ngay_Tao = DateTimeOffset.Now,
                Nguoi_Tao = dto.Nguoi_Tao
            };

            await _quangRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(QuangUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _quangRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            // Validate business rules
            if (await _quangRepo.ExistsByCodeAsync(dto.Ma_Quang, ct))
            {
                var existingEntity = await _quangRepo.FindAsync(x => x.Ma_Quang == dto.Ma_Quang, false, ct);
                if (existingEntity.FirstOrDefault()?.ID != dto.ID)
                    throw new InvalidOperationException($"Mã quặng '{dto.Ma_Quang}' đã tồn tại.");
            }

            entity.Ma_Quang = dto.Ma_Quang;
            entity.Ten_Quang = dto.Ten_Quang;
            entity.Loai_Quang = dto.Loai_Quang;
            entity.Dang_Hoat_Dong = dto.Dang_Hoat_Dong;
            entity.Ghi_Chu = dto.Ghi_Chu;
            entity.Ngay_Sua = DateTimeOffset.Now;
            entity.Nguoi_Sua = null; // TODO: Get from current user context

            _quangRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(QuangUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.Quang, ct);
            }
            else
            {
                var updateDto = new QuangUpdateDto(
                    dto.ID.Value,
                    dto.Quang.Ma_Quang,
                    dto.Quang.Ten_Quang,
                    dto.Quang.Loai_Quang,
                    dto.Quang.Dang_Hoat_Dong,
                    dto.Quang.Ghi_Chu
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quangRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _quangRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _quangRepo.DeleteQuangWithRelatedDataAsync(id, _congThucRepo, ct);
        }

        public async Task<bool> ExistsByCodeAsync(string maQuang, CancellationToken ct = default)
            => await _quangRepo.ExistsByCodeAsync(maQuang, ct);

        public async Task<bool> ExistsByCodeOrNameAsync(string maQuang, string? tenQuang, int? excludeId = null, CancellationToken ct = default)
            => await _quangRepo.ExistsByCodeOrNameAsync(maQuang, tenQuang, excludeId, ct);

        public async Task<IReadOnlyList<QuangResponse>> GetByLoaiAsync(int loaiQuang, CancellationToken ct = default)
        {
            var entities = await _quangRepo.GetByLoaiAsync(loaiQuang, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<IReadOnlyList<QuangResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _quangRepo.GetActiveAsync(ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        {
            var entity = await _quangRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Dang_Hoat_Dong = isActive;
            _quangRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }



        public async Task<int> UpsertWithThanhPhanAsync(QuangUpsertWithThanhPhanDto dto, CancellationToken ct = default)
        {
            return await _quangRepo.UpsertWithThanhPhanAsync(dto, ct);
        }

        public async Task<IReadOnlyList<OreChemistryBatchItem>> GetOreChemistryBatchAsync(IReadOnlyList<int> quangIds, CancellationToken ct = default)
        {
            return await _quangRepo.GetOreChemistryBatchAsync(quangIds, ct);
        }

        public Task<IReadOnlyList<FormulaByOutputOreResponse>> GetFormulasByOutputOreIdsAsync(IReadOnlyList<int> outputOreIds, CancellationToken ct = default)
        {
            return _quangRepo.GetFormulasByOutputOreIdsAsync(outputOreIds, ct);
        }

        public async Task<int?> GetSlagIdByGangIdAsync(int gangId, CancellationToken ct = default)
        {
            return await _quangRepo.GetSlagIdByGangIdAsync(gangId, ct);
        }

        public async Task<(QuangDetailResponse gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryAsync(int gangId, CancellationToken ct = default)
        {
            var gang = await _quangRepo.GetDetailByIdAsync(gangId, ct);
            if (gang is null) throw new InvalidOperationException($"Không tìm thấy quặng gang ID={gangId}");

            var slagId = await _quangRepo.GetSlagIdByGangIdAsync(gangId, ct);
            QuangDetailResponse? slag = null;
            if (slagId.HasValue)
            {
                slag = await _quangRepo.GetDetailByIdAsync(slagId.Value, ct);
            }

            return (gang, slag);
        }

        public async Task<(QuangDetailResponse? gang, QuangDetailResponse? slag)> GetGangAndSlagChemistryByPlanAsync(int planId, CancellationToken ct = default)
        {
            return await _quangRepo.GetGangAndSlagChemistryByPlanAsync(planId, ct);
        }

        public async Task<QuangDetailResponse?> GetLatestGangTargetAsync(CancellationToken ct = default)
        {
            return await _quangRepo.GetLatestGangTargetAsync(ct);
        }

        public async Task<GangTemplateConfigResponse?> GetGangTemplateConfigAsync(int? gangId = null, CancellationToken ct = default)
        {
            return await _quangRepo.GetGangTemplateConfigAsync(gangId, ct);
        }

        public Task<GangDichConfigDetailResponse?> GetGangDichDetailWithConfigAsync(int gangId, CancellationToken ct = default)
        {
            return _quangRepo.GetGangDichDetailWithConfigAsync(gangId, ct);
        }

        private static QuangResponse MapToResponse(Quang entity) => new(
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
            null, // Gia_USD_1Tan
            null, // Gia_VND_1Tan
            null, // Ty_Gia_USD_VND
            null, // Ngay_Chon_TyGia
            null, // Tien_Te
            entity.ID_Quang_Gang
        );

        public async Task<int> UpsertKetQuaWithThanhPhanAsync(QuangKetQuaUpsertDto dto, CancellationToken ct = default)
        {
            return await _quangRepo.UpsertKetQuaWithThanhPhanAsync(dto, ct);
        }

        public Task<int> UpsertGangDichWithConfigAsync(GangDichConfigUpsertDto dto, CancellationToken ct = default)
        {
            return _quangRepo.UpsertGangDichWithConfigAsync(dto, ct);
        }

        public async Task<bool> DeleteGangDichWithRelatedDataAsync(int gangDichId, CancellationToken ct = default)
        {
            return await _quangRepo.DeleteGangDichWithRelatedDataAsync(gangDichId, _phuongAnRepo, _congThucRepo, ct);
        }
    }
}
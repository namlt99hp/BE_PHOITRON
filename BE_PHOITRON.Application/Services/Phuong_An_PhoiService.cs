using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class Phuong_An_PhoiService : IPhuong_An_PhoiService
    {
        private readonly IPhuong_An_PhoiRepository _phuongAnRepo;
        private readonly IPA_LuaChon_CongThucRepository _paLuaChonRepo;
        private readonly IUnitOfWork _uow;

        public Phuong_An_PhoiService(
            IPhuong_An_PhoiRepository phuongAnRepo,
            IPA_LuaChon_CongThucRepository paLuaChonRepo,
            IUnitOfWork uow)
        {
            _phuongAnRepo = phuongAnRepo;
            _paLuaChonRepo = paLuaChonRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<Phuong_An_PhoiResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _phuongAnRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<Phuong_An_PhoiResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _phuongAnRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(Phuong_An_PhoiCreateDto dto, CancellationToken ct = default)
        {
            var entity = new Phuong_An_Phoi
            {
                Ten_Phuong_An = dto.Ten_Phuong_An,
                ID_Quang_Dich = dto.ID_Quang_Dich,
                Phien_Ban = dto.Phien_Ban,
                Trang_Thai = dto.Trang_Thai,
                Ngay_Tinh_Toan = dto.Ngay_Tinh_Toan,
                Muc_Tieu = dto.Muc_Tieu,
                Ghi_Chu = dto.Ghi_Chu,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = null // TODO: Get from current user context
            };

            await _phuongAnRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(Phuong_An_PhoiUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _phuongAnRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            entity.Ten_Phuong_An = dto.Ten_Phuong_An;
            entity.ID_Quang_Dich = dto.ID_Quang_Dich;
            entity.Phien_Ban = dto.Phien_Ban;
            entity.Trang_Thai = dto.Trang_Thai;
            entity.Ngay_Tinh_Toan = dto.Ngay_Tinh_Toan;
            entity.Muc_Tieu = dto.Muc_Tieu;
            entity.Ghi_Chu = dto.Ghi_Chu;
            entity.UpdatedAt = DateTimeOffset.Now;
            entity.UpdatedBy = null; // TODO: Get from current user context

            _phuongAnRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(Phuong_An_PhoiUpsertDto dto, CancellationToken ct = default)
        {
            // Delegate to repository for complete upsert logic including ore cloning
            return await _phuongAnRepo.UpsertPhuongAnPhoiAsync(dto, ct);
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            return await _phuongAnRepo.DeletePlanAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            return await _phuongAnRepo.DeletePlanAsync(id, ct);
        }

        public async Task<bool> DeletePlanWithRelatedDataAsync(int planId, CancellationToken ct = default)
        {
            return await _phuongAnRepo.DeletePlanWithRelatedDataAsync(planId, ct);
        }

        public async Task<IReadOnlyList<Phuong_An_PhoiResponse>> GetByQuangDichAsync(int idQuangDich, CancellationToken ct = default)
        {
            var entities = await _phuongAnRepo.GetByQuangDichAsync(idQuangDich, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<IReadOnlyList<Phuong_An_PhoiResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _phuongAnRepo.GetActiveAsync(ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> ValidateNoCircularDependencyAsync(int idPhuongAn, CancellationToken ct = default)
            => await _paLuaChonRepo.ValidateNoCircularDependencyAsync(idPhuongAn, ct);

        public async Task<PhuongAnTinhToanResponse?> TinhToanPhuongAnAsync(int idPhuongAn, CancellationToken ct = default)
        {
            // TODO: Implement complex calculation logic
            // This will involve:
            // 1. Get all recipe selections for the plan
            // 2. Build dependency tree
            // 3. Calculate quantities and costs
            // 4. Calculate final TPHH composition
            // 5. Return comprehensive result
            
            var phuongAn = await GetByIdAsync(idPhuongAn, ct);
            if (phuongAn is null) return null;

            // Placeholder implementation
            return new PhuongAnTinhToanResponse(
                phuongAn.ID,
                phuongAn.Ten_Phuong_An,
                phuongAn.ID_Quang_Dich,
                phuongAn.Quang_Dich_Ma ?? "",
                phuongAn.Quang_Dich_Ten ?? "",
                phuongAn.Ngay_Tinh_Toan,
                1.0m, // Khoi_Luong_DauRa
                0m,   // Tong_Chi_Phi_1Tan
                new Dictionary<string, decimal>(), // TPHH_DauRa
                new Dictionary<string, decimal>(), // Co_Cau_Quang_Tho
                new List<CongThucChiTietResponse>() // Cong_Thuc_Chi_Tiet
            );
        }

        public async Task<SoSanhPhuongAnResponse> SoSanhPhuongAnAsync(List<int> idPhuongAn, CancellationToken ct = default)
        {
            // TODO: Implement comparison logic
            var phuongAnList = new List<PhuongAnTinhToanResponse>();
            
            foreach (var id in idPhuongAn)
            {
                var phuongAn = await TinhToanPhuongAnAsync(id, ct);
                if (phuongAn is not null)
                    phuongAnList.Add(phuongAn);
            }

            var thongKe = new Dictionary<string, object>
            {
                ["So_Luong_Phuong_An"] = phuongAnList.Count,
                ["Chi_Phi_Thap_Nhat"] = phuongAnList.Min(x => x.Tong_Chi_Phi_1Tan),
                ["Chi_Phi_Cao_Nhat"] = phuongAnList.Max(x => x.Tong_Chi_Phi_1Tan)
            };

            var phuongAnToiUu = phuongAnList.OrderBy(x => x.Tong_Chi_Phi_1Tan).FirstOrDefault();

            return new SoSanhPhuongAnResponse(phuongAnList, thongKe, phuongAnToiUu);
        }

        // Uỷ quyền cho Repository để đảm bảo tất cả truy cập DbContext tập trung một nơi
        public Task<int> MixAsync(MixQuangRequestDto dto, CancellationToken ct = default)
            => _phuongAnRepo.MixAsync(dto, ct);

        public Task<int> MixWithCompleteDataAsync(MixWithCompleteDataDto dto, CancellationToken ct = default)
            => _phuongAnRepo.MixWithCompleteDataAsync(dto, ct);

        public Task<CongThucPhoiDetailResponse?> GetCongThucPhoiDetailAsync(int congThucPhoiId, CancellationToken ct = default)
            => _phuongAnRepo.GetCongThucPhoiDetailAsync(congThucPhoiId, ct);

        public Task<PhuongAnWithFormulasResponse?> GetFormulasByPlanAsync(int idPhuongAn, CancellationToken ct = default)
            => _phuongAnRepo.GetFormulasByPlanAsync(idPhuongAn, ct);

        public Task<PhuongAnWithMilestonesResponse?> GetFormulasByPlanWithDetailsAsync(int idPhuongAn, CancellationToken ct = default)
            => _phuongAnRepo.GetFormulasByPlanWithDetailsAsync(idPhuongAn, ct);


        public Task<CongThucPhoiDetailMinimal?> GetDetailMinimalAsync(int congThucPhoiId, CancellationToken ct = default)
            => _phuongAnRepo.GetDetailMinimalAsync(congThucPhoiId, ct);

        public Task<int> ClonePlanAsync(ClonePlanRequestDto dto, CancellationToken ct = default)
            => _phuongAnRepo.ClonePlanAsync(dto, ct);

        public Task<int> CloneMilestonesAsync(CloneMilestonesRequestDto dto, CancellationToken ct = default)
            => _phuongAnRepo.CloneMilestonesAsync(dto, ct);

        public Task<List<PlanSectionDto>> GetPlanSectionsByGangDichAsync(int gangDichId, bool includeThieuKet = true, bool includeLoCao = true, CancellationToken ct = default)
            => _phuongAnRepo.GetPlanSectionsByGangDichAsync(gangDichId, includeThieuKet, includeLoCao, ct);

       

        private static Phuong_An_PhoiResponse MapToResponse(Phuong_An_Phoi entity) => new(
            entity.ID,
            entity.Ten_Phuong_An,
            entity.ID_Quang_Dich,
            entity.Phien_Ban,
            entity.Trang_Thai,
            entity.Ngay_Tinh_Toan,
            entity.Muc_Tieu,
            entity.Ghi_Chu,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy,
            // Navigation properties - will be populated by repository if needed
            null, // Quang_Dich_Ma
            null, // Quang_Dich_Ten
            // Calculated properties - will be calculated by repository if needed
            null, // So_Luong_Cong_Thuc
            null, // Tong_Chi_Phi_1Tan
            null  // Co_Vong_Lap
        );
    }
}

using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Services
{
    public class CongThucPhoiService : ICongThucPhoiService
    {
        private readonly ICongThucPhoiRepository _congthucphoiRepo;
        private readonly IUnitOfWork _uow;

        public CongThucPhoiService(ICongThucPhoiRepository congthucphoiRepo, IUnitOfWork uow)
        {
            _congthucphoiRepo = congthucphoiRepo; _uow = uow;
        }
        public async Task<int> CreateAsync(CreateCongThucPhoiDto dto, CancellationToken ct = default)
        {
            if (await _congthucphoiRepo.ExistsByCodeAsync(dto.MaCongThuc, ct))
                throw new InvalidOperationException("Ma Công thức đã tồn tại.");

            var e = new CongThucPhoi { MaCongThuc = dto.MaCongThuc, TenCongThuc= dto.TenCongThuc, GhiChu = dto.GhiChu, NgayTao = DateTime.UtcNow };
            await _congthucphoiRepo.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);
            return e.ID;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var e = await _congthucphoiRepo.GetByIdAsync(id, ct);
            if (e is null) return false;
            _congthucphoiRepo.Remove(e);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<CongThucPhoiResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var e = await _congthucphoiRepo.GetByIdAsync(id, ct);
            return e is null ? null : Map(e);
        }

        public async Task<(int total, IReadOnlyList<CongThucPhoiResponse> data)> ListAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default)
        {
            var (total, entities) = await _congthucphoiRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(Map).ToList();
            return (total, data);
        }

        public Task<bool> UpdateAsync(int id, UpdateCongThucPhoiDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        private static CongThucPhoiResponse Map(CongThucPhoi e) => new(e.ID, e.MaCongThuc, e.TenCongThuc, e.TongPhanTram, e.GhiChu, e.NgayTao, e.ID_NguoiTao, e.NgaySua, e.ID_NguoiSua, e.IsDeleted, e.ID_QuangNeo);

        public async Task<int> UpdateCongThucPTDto(UpdateCongThucPTDto dto, CancellationToken ct = default)
        {
            var result = await _congthucphoiRepo.UpdateCongThucPTDto(dto, ct);
            await _uow.SaveChangesAsync(ct);
            return result;
        }

        public async Task<CongThucPhoiDetailRespone> GetCongThucPhoiDetail(int id, CancellationToken ct = default)
        {
            var result = await _congthucphoiRepo.GetCongThucPhoiDetailAsync(id,ct);
            return result;
        }

        public async Task<int> UpsertCongThucPhoiTron(UpsertCongThucPTDto dto, CancellationToken ct = default)
        {
            var result = await _congthucphoiRepo.UpsertCongThucPTAsync(dto, ct);
            await _uow.SaveChangesAsync(ct);
            return result;
        }

        //======================
        public async Task<UpsertAndConfirmResult> UpsertAndConfirmAsync(UpsertAndConfirmDto dto, CancellationToken ct = default)
        {
            var result = await _congthucphoiRepo.UpsertAndConfirmAsync(dto, ct);
            await _uow.SaveChangesAsync(ct);
            return result;
        }

        public async Task<CongThucEditVm?> GetForEditAsync(int formulaId, CancellationToken ct = default)
        {
            var result = await _congthucphoiRepo.GetForEditAsync(formulaId, ct);
            await _uow.SaveChangesAsync(ct);
            return result;
        }

        public async Task<NeoDashboardVm?> GetByNeoAsync(int quangNeoId, CancellationToken ct = default)
        {
            var result = await _congthucphoiRepo.GetByNeoAsync(quangNeoId, ct);
            await _uow.SaveChangesAsync(ct);
            return result;
        }
    }
}

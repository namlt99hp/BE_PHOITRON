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
    public class TPHHService : ITPHHService
    {
        private readonly ITPHHRepository _tphhRepo;
        private readonly IUnitOfWork _uow;

        public TPHHService(ITPHHRepository tphhRepo, IUnitOfWork uow)
        {
            _tphhRepo = tphhRepo; _uow = uow;
        }
        public async Task<int> CreateAsync(TPHHCreateDto dto, CancellationToken ct = default)
        {
            if(await _tphhRepo.ExistsByCodeAsync(dto.Ma_TPHH,ct))
                throw new InvalidOperationException("Ma TPHH đã tồn tại.");

            var e = new TP_HoaHoc { Ma_TPHH = dto.Ma_TPHH, Ten_TPHH = dto.Ten_TPHH, GhiChu = dto.GhiChu, NgayTao = DateTime.UtcNow };
            await _tphhRepo.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);
            return e.ID;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var e = await _tphhRepo.GetByIdAsync(id, ct);
            if (e is null) return false;
            _tphhRepo.Remove(e);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<TPHHResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var e = await _tphhRepo.GetByIdAsync(id, ct);
            return e is null ? null : Map(e);
        }

        public async Task<(int total, IReadOnlyList<TPHHResponse> data)> ListAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default)
        {
            var (total, entities) = await _tphhRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(Map).ToList();
            return (total, data);
        }
        private static TPHHResponse Map(TP_HoaHoc e) => new(e.ID, e.Ma_TPHH, e.Ten_TPHH, e.GhiChu, e.NgayTao, e.ID_NguoiTao, e.NgaySua, e.ID_NguoiSua, e.IsDeleted);
        public async Task<bool> UpdateAsync(int id, TPHHUpdateDto dto, CancellationToken ct = default)
        {
            var e = await _tphhRepo.GetByIdAsync(id, ct);
            if (e is null) return false;
            e.Ma_TPHH = dto.Ma_TPHH; 
            e.Ten_TPHH = dto.Ten_TPHH;
            e.GhiChu = dto.GhiChu;
            e.NgaySua = DateTime.UtcNow;

            _tphhRepo.Update(e);
            await _uow.SaveChangesAsync(ct);
            return true;
        }
        public async Task<IReadOnlyList<TPHHItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default)
        {
            var result = await _tphhRepo.GetByListIdsAsync(IDs, ct);
            return result;
        }
    }
}

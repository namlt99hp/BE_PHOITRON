using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModel;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.DataEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Services
{
    public class QuangService : IQuangService
    {
        private readonly IQuangRepository _quangRepo;
        private readonly IUnitOfWork _uow;

        public QuangService(IQuangRepository quangRepo, IUnitOfWork uow)
        {
            _quangRepo = quangRepo; _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<QuangResponse> data)> ListAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir, CancellationToken ct = default)
        {
            var (total, entities) = await _quangRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(Map).ToList();
            return (total, data);
        }

        public async Task<QuangResponse?> GetAsync(int id, CancellationToken ct = default)
        {
            var e = await _quangRepo.GetByIdAsync(id, ct);
            return e is null ? null : Map(e);
        }

        public async Task<int> CreateAsync(QuangCreateDto dto, CancellationToken ct = default)
        {
            if (await _quangRepo.ExistsByCodeAsync(dto.MaQuang, ct))
                throw new InvalidOperationException("MaQuang đã tồn tại.");

            var e = new Quang { MaQuang = dto.MaQuang, TenQuang = dto.TenQuang, Gia = dto.Gia, GhiChu = dto.GhiChu };
            await _quangRepo.AddAsync(e, ct);
            await _uow.SaveChangesAsync(ct);
            return e.ID;
        }

        public async Task<bool> UpdateAsync(int id, QuangUpdateDto dto, CancellationToken ct = default)
        {
            var e = await _quangRepo.GetByIdAsync(id, ct);
            if (e is null) return false;
            e.TenQuang = dto.TenQuang;
            e.Gia = dto.Gia;
            e.GhiChu = dto.GhiChu;
            e.NgaySua = DateTime.UtcNow;

            _quangRepo.Update(e);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var e = await _quangRepo.GetByIdAsync(id, ct);
            if (e is null) return false;
            _quangRepo.Remove(e);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpdateTPHH(Quang_TPHHUpdateDto dto, CancellationToken ct = default)
        {
            var idQuangUpdated = await _quangRepo.UpdateTPHH(dto, ct);
            await _uow.SaveChangesAsync(ct);
            return idQuangUpdated;
        }


        private static QuangResponse Map(Quang e) => new(e.ID, e.MaQuang, e.TenQuang, e.Gia, e.GhiChu, e.NgayTao, e.ID_NguoiTao, e.NgaySua, e.ID_NguoiSua, e.IsDeleted, e.MatKhiNung, e.LoaiQuang, e.ID_CongThucPhoi);

        public async Task<QuangDetailResponse> GetDetailQuang(int id, CancellationToken ct = default)
        {
            var result = await _quangRepo.GetDetailQuang(id, ct);
            return result;
        }

        public async Task<int> UpsertAsync(UpsertQuangMuaDto dto, CancellationToken ct = default)
        {
            var id = await _quangRepo.UpsertAsync(dto, ct);
            await _uow.SaveChangesAsync(ct);
            return id;
        }

        public async Task<List<QuangDetailResponse>> getOreChemistryBatch(List<int> id_Quangs, CancellationToken ct = default)
        {
            var result = await _quangRepo.getOreChemistryBatch(id_Quangs, ct);
            return result;
        }

        public async Task<IReadOnlyList<QuangItemResponse>> GetByListIdsAsync(List<int> IDs, CancellationToken ct = default)
        {
            var result = await _quangRepo.GetByListIdsAsync(IDs, ct);
            return result;
        }
    }
}

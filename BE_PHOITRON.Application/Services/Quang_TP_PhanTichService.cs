using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class Quang_TP_PhanTichService : IQuang_TP_PhanTichService
    {
        private readonly IQuang_TP_PhanTichRepository _quangTPRepo;
        private readonly IUnitOfWork _uow;

        public Quang_TP_PhanTichService(IQuang_TP_PhanTichRepository quangTPRepo, IUnitOfWork uow)
        {
            _quangTPRepo = quangTPRepo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<Quang_TP_PhanTichResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _quangTPRepo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<Quang_TP_PhanTichResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quangTPRepo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> CreateAsync(Quang_TP_PhanTichCreateDto dto, CancellationToken ct = default)
        {
            // Validate business rules
            if (await _quangTPRepo.HasOverlappingPeriodAsync(dto.ID_Quang, dto.ID_TPHH, dto.Hieu_Luc_Tu, dto.Hieu_Luc_Den, null, ct))
                throw new InvalidOperationException($"Phân tích thành phần này đã có hiệu lực trong khoảng thời gian này.");

            var entity = new Quang_TP_PhanTich
            {
                ID_Quang = dto.ID_Quang,
                ID_TPHH = dto.ID_TPHH,
                Gia_Tri_PhanTram = dto.Gia_Tri_PhanTram,
                Hieu_Luc_Tu = dto.Hieu_Luc_Tu,
                Hieu_Luc_Den = dto.Hieu_Luc_Den,
                Nguon_Du_Lieu = dto.Nguon_Du_Lieu,
                Ghi_Chu = dto.Ghi_Chu,
                ThuTuTPHH = dto.ThuTuTPHH,
                KhoiLuong = dto.KhoiLuong,
                CalcFormula = dto.CalcFormula,
                IsCalculated = dto.IsCalculated
            };

            await _quangTPRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ID;
        }

        public async Task<bool> UpdateAsync(Quang_TP_PhanTichUpdateDto dto, CancellationToken ct = default)
        {
            var entity = await _quangTPRepo.GetByIdAsync(dto.ID, ct);
            if (entity is null) return false;

            // Validate business rules
            if (await _quangTPRepo.HasOverlappingPeriodAsync(dto.ID_Quang, dto.ID_TPHH, dto.Hieu_Luc_Tu, dto.Hieu_Luc_Den, dto.ID, ct))
                throw new InvalidOperationException($"Phân tích thành phần này đã có hiệu lực trong khoảng thời gian này.");

            entity.ID_Quang = dto.ID_Quang;
            entity.ID_TPHH = dto.ID_TPHH;
            entity.Gia_Tri_PhanTram = dto.Gia_Tri_PhanTram;
            entity.Hieu_Luc_Tu = dto.Hieu_Luc_Tu;
            entity.Hieu_Luc_Den = dto.Hieu_Luc_Den;
            entity.Nguon_Du_Lieu = dto.Nguon_Du_Lieu;
            entity.Ghi_Chu = dto.Ghi_Chu;
            entity.ThuTuTPHH = dto.ThuTuTPHH;
            entity.KhoiLuong = dto.KhoiLuong;
            entity.CalcFormula = dto.CalcFormula;
            entity.IsCalculated = dto.IsCalculated;

            _quangTPRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> UpsertAsync(Quang_TP_PhanTichUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                return await CreateAsync(dto.Quang_TP_PhanTich, ct);
            }
            else
            {
                var updateDto = new Quang_TP_PhanTichUpdateDto(
                    dto.ID.Value,
                    dto.Quang_TP_PhanTich.ID_Quang,
                    dto.Quang_TP_PhanTich.ID_TPHH,
                    dto.Quang_TP_PhanTich.Gia_Tri_PhanTram,
                    dto.Quang_TP_PhanTich.Hieu_Luc_Tu,
                    dto.Quang_TP_PhanTich.Hieu_Luc_Den,
                    dto.Quang_TP_PhanTich.Nguon_Du_Lieu,
                    dto.Quang_TP_PhanTich.Ghi_Chu,
                    dto.Quang_TP_PhanTich.ThuTuTPHH,
                    dto.Quang_TP_PhanTich.KhoiLuong,
                    dto.Quang_TP_PhanTich.CalcFormula,
                    dto.Quang_TP_PhanTich.IsCalculated
                );
                var success = await UpdateAsync(updateDto, ct);
                return success ? dto.ID.Value : 0;
            }
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _quangTPRepo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            entity.Da_Xoa = true;
            _quangTPRepo.Update(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IReadOnlyList<Quang_TP_PhanTichResponse>> GetByQuangAndDateAsync(int idQuang, DateTimeOffset ngayTinh, CancellationToken ct = default)
        {
            var entities = await _quangTPRepo.GetByQuangAndDateAsync(idQuang, ngayTinh, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<IReadOnlyList<Quang_TP_PhanTichResponse>> GetByQuangAsync(int idQuang, CancellationToken ct = default)
        {
            var entities = await _quangTPRepo.GetByQuangAsync(idQuang, ct);
            return entities.Select(MapToResponse).ToList();
        }

        public async Task<bool> HasOverlappingPeriodAsync(int idQuang, int idTPHH, DateTimeOffset hieuLucTu, DateTimeOffset? hieuLucDen, int? excludeId = null, CancellationToken ct = default)
            => await _quangTPRepo.HasOverlappingPeriodAsync(idQuang, idTPHH, hieuLucTu, hieuLucDen, excludeId, ct);

        public async Task<Dictionary<int, decimal>> CalculateTPHHFormulasAsync(int quangId, CancellationToken ct = default)
        {
            return await _quangTPRepo.CalculateTPHHFormulasAsync(quangId, ct);
        }


        private static Quang_TP_PhanTichResponse MapToResponse(Quang_TP_PhanTich entity) => new(
            entity.ID,
            entity.ID_Quang,
            entity.ID_TPHH,
            entity.Gia_Tri_PhanTram,
            entity.Hieu_Luc_Tu,
            entity.Hieu_Luc_Den,
            entity.Nguon_Du_Lieu,
            entity.Ghi_Chu,
            entity.ThuTuTPHH,
            entity.Da_Xoa,
            entity.KhoiLuong,
            entity.CalcFormula,
            entity.IsCalculated,
            // Navigation properties - will be populated by repository if needed
            null, // Quang_Ma
            null, // Quang_Ten
            null, // TP_HoaHoc_Ma
            null  // TP_HoaHoc_Ten
        );
    }
}

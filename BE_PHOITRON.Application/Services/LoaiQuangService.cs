using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class LoaiQuangService : ILoaiQuangService
    {
        private readonly ILoaiQuangRepository _repo;
        private readonly IUnitOfWork _uow;

        public LoaiQuangService(ILoaiQuangRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<LoaiQuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _repo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<LoaiQuangResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> UpsertAsync(LoaiQuangUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                // Create
                if (await _repo.ExistsByCodeAsync(dto.LoaiQuang.MaLoaiQuang, ct))
                    throw new InvalidOperationException($"Mã loại quặng '{dto.LoaiQuang.MaLoaiQuang}' đã tồn tại.");

                var entity = new LoaiQuang
                {
                    MaLoaiQuang = dto.LoaiQuang.MaLoaiQuang,
                    TenLoaiQuang = dto.LoaiQuang.TenLoaiQuang,
                    MoTa = dto.LoaiQuang.MoTa,
                    IsActive = dto.LoaiQuang.IsActive,
                    NgayTao = DateTimeOffset.Now,
                    NguoiTao = dto.LoaiQuang.NguoiTao
                };

                await _repo.AddAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);
                return entity.ID;
            }
            else
            {
                // Update
                var entity = await _repo.GetByIdAsync(dto.ID.Value, ct);
                if (entity is null) return 0;

                if (!string.Equals(entity.MaLoaiQuang, dto.LoaiQuang.MaLoaiQuang, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _repo.ExistsByCodeAsync(dto.LoaiQuang.MaLoaiQuang, ct))
                        throw new InvalidOperationException($"Mã loại quặng '{dto.LoaiQuang.MaLoaiQuang}' đã tồn tại.");
                }

                entity.MaLoaiQuang = dto.LoaiQuang.MaLoaiQuang;
                entity.TenLoaiQuang = dto.LoaiQuang.TenLoaiQuang;
                entity.MoTa = dto.LoaiQuang.MoTa;
                entity.IsActive = dto.LoaiQuang.IsActive;
                entity.NgaySua = DateTimeOffset.Now;
                entity.NguoiSua = dto.LoaiQuang.NguoiTao;

                _repo.Update(entity);
                await _uow.SaveChangesAsync(ct);
                return entity.ID;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            _repo.Remove(entity);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IReadOnlyList<LoaiQuangResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _repo.GetActiveAsync(ct);
            return entities.Select(MapToResponse).ToList();
        }

        private static LoaiQuangResponse MapToResponse(LoaiQuang entity) => new(
            entity.ID,
            entity.MaLoaiQuang,
            entity.TenLoaiQuang,
            entity.MoTa,
            entity.IsActive,
            entity.NgayTao,
            entity.NguoiTao,
            entity.NgaySua,
            entity.NguoiSua
        );
    }
}


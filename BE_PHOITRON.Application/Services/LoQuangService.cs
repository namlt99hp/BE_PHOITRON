using BE_PHOITRON.Application.Abstractions;
using BE_PHOITRON.Application.Abstractions.Repositories;
using BE_PHOITRON.Application.DTOs;
using BE_PHOITRON.Application.ResponsesModels;
using BE_PHOITRON.Application.Services.Interfaces;
using BE_PHOITRON.Domain.Entities;

namespace BE_PHOITRON.Application.Services
{
    public class LoQuangService : ILoQuangService
    {
        private readonly ILoQuangRepository _repo;
        private readonly IUnitOfWork _uow;

        public LoQuangService(ILoQuangRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<(int total, IReadOnlyList<LoQuangResponse> data)> SearchPagedAsync(
            int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null, CancellationToken ct = default)
        {
            var (total, entities) = await _repo.SearchPagedAsync(page, pageSize, search, sortBy, sortDir, ct);
            var data = entities.Select(MapToResponse).ToList();
            return (total, data);
        }

        public async Task<LoQuangResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<int> UpsertAsync(LoQuangUpsertDto dto, CancellationToken ct = default)
        {
            if (dto.ID is null or 0)
            {
                // Create
                if (await _repo.ExistsByCodeAsync(dto.LoQuang.MaLoQuang, ct))
                    throw new InvalidOperationException($"Mã lô quặng '{dto.LoQuang.MaLoQuang}' đã tồn tại.");

                var entity = new LoQuang
                {
                    MaLoQuang = dto.LoQuang.MaLoQuang,
                    MoTa = dto.LoQuang.MoTa,
                    IsActive = dto.LoQuang.IsActive,
                    NgayTao = DateTimeOffset.Now,
                    NguoiTao = dto.LoQuang.NguoiTao
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

                if (!string.Equals(entity.MaLoQuang, dto.LoQuang.MaLoQuang, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _repo.ExistsByCodeAsync(dto.LoQuang.MaLoQuang, ct))
                        throw new InvalidOperationException($"Mã lô quặng '{dto.LoQuang.MaLoQuang}' đã tồn tại.");
                }

                entity.MaLoQuang = dto.LoQuang.MaLoQuang;
                entity.MoTa = dto.LoQuang.MoTa;
                entity.IsActive = dto.LoQuang.IsActive;
                entity.NgaySua = DateTimeOffset.Now;
                entity.NguoiSua = dto.LoQuang.NguoiTao;

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

        public async Task<IReadOnlyList<LoQuangResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var entities = await _repo.GetActiveAsync(ct);
            return entities.Select(MapToResponse).ToList();
        }

        private static LoQuangResponse MapToResponse(LoQuang entity) => new(
            entity.ID,
            entity.MaLoQuang,
            entity.MoTa,
            entity.IsActive,
            entity.NgayTao,
            entity.NguoiTao,
            entity.NgaySua,
            entity.NguoiSua
        );
    }
}


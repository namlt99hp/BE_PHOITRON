using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        public BaseRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
            => _set.FindAsync(new[] { id }, ct).AsTask();

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().ToListAsync(ct);

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default)
        {
            var q = _set.Where(predicate);
            if (asNoTracking) q = q.AsNoTracking();
            return await q.ToListAsync(ct);
        }

        public IQueryable<T> Query(bool asNoTracking = true)
            => asNoTracking ? _set.AsNoTracking() : _set;

        public Task AddAsync(T entity, CancellationToken ct = default)
            => _set.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _set.AddRangeAsync(entities, ct);

        public void Update(T entity) => _set.Update(entity);
        public void UpdateRange(IEnumerable<T> entities) => _set.UpdateRange(entities);

        public void Remove(T entity) => _set.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => _set.AnyAsync(predicate, ct);

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null ? _set.CountAsync(ct) : _set.CountAsync(predicate, ct);
    }
}

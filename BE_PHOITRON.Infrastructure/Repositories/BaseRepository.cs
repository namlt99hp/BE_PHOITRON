using BE_PHOITRON.Application.Abstractions.Base;
using BE_PHOITRON.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        // READ operations
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

        // CREATE operations
        public Task AddAsync(T entity, CancellationToken ct = default)
            => _set.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _set.AddRangeAsync(entities, ct);

        // UPDATE operations
        public void Update(T entity) => _set.Update(entity);
        public void UpdateRange(IEnumerable<T> entities) => _set.UpdateRange(entities);

        // DELETE operations (soft delete for entities with Da_Xoa field)
        public void Remove(T entity) => _set.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);

        // UTILITY operations
        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => _set.AnyAsync(predicate, ct);

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null ? _set.CountAsync(ct) : _set.CountAsync(predicate, ct);

        // SOFT DELETE operations (for entities with Da_Xoa field)
        public virtual async Task<bool> SoftDeleteAsync(object id, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id, ct);
            if (entity == null) return false;

            // Use reflection to set Da_Xoa = true
            var daXoaProperty = typeof(T).GetProperty("Da_Xoa");
            if (daXoaProperty != null && daXoaProperty.CanWrite)
            {
                daXoaProperty.SetValue(entity, true);
                Update(entity);
                return true;
            }
            return false;
        }

        // ACTIVE/INACTIVE operations (for entities with Dang_Hoat_Dong field)
        public virtual async Task<bool> SetActiveAsync(object id, bool isActive, CancellationToken ct = default)
        {
            var entity = await GetByIdAsync(id, ct);
            if (entity == null) return false;

            var dangHoatDongProperty = typeof(T).GetProperty("Dang_Hoat_Dong");
            if (dangHoatDongProperty != null && dangHoatDongProperty.CanWrite)
            {
                dangHoatDongProperty.SetValue(entity, isActive);
                Update(entity);
                return true;
            }
            return false;
        }

        // SEARCH with pagination
        public virtual async Task<(int total, IReadOnlyList<T> data)> SearchPagedAsync(
            int page, 
            int pageSize, 
            string? search = null, 
            string? sortBy = null, 
            string? sortDir = null, 
            CancellationToken ct = default)
        {
            page = Math.Max(0, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _set.AsNoTracking();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = ApplySearchFilter(query, search);
            }

            // Apply soft delete filter (if entity has Da_Xoa field)
            query = ApplySoftDeleteFilter(query);

            var total = await query.CountAsync(ct);

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = ApplySorting(query, sortBy, sortDir);
            }

            var data = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (total, data);
        }

        // Virtual methods to be overridden in derived classes
        protected virtual IQueryable<T> ApplySearchFilter(IQueryable<T> query, string search)
        {
            // Default implementation - can be overridden in derived classes
            return query;
        }

        protected virtual IQueryable<T> ApplySoftDeleteFilter(IQueryable<T> query)
        {
            // Check if entity has Da_Xoa property
            var daXoaProperty = typeof(T).GetProperty("Da_Xoa");
            if (daXoaProperty != null)
            {
                // Apply filter: Da_Xoa = false
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, daXoaProperty);
                var constant = Expression.Constant(false);
                var equal = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);
                return query.Where(lambda);
            }
            return query;
        }

        protected virtual IQueryable<T> ApplySorting(IQueryable<T> query, string sortBy, string? sortDir)
        {
            // Default sorting by ID descending
            return query.OrderByDescending(x => EF.Property<object>(x, "ID"));
        }
    }
}

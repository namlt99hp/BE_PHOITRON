using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Abstractions.Base
{
    public interface IRepository<T> where T : class
    {
        // READ
        Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken ct = default);
        IQueryable<T> Query(bool asNoTracking = true);

        // CREATE
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        // UPDATE
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        // DELETE
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        // UTIL
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    }
}


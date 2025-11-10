using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Domain.Interfaces;

namespace SalesAnalysis.Persistence.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext Context;
        protected readonly DbSet<T> DbSet;

        protected BaseRepository(DbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = Context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<int> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
            return await Context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbSet.Update(entity);
            return await Context.SaveChangesAsync(cancellationToken) > 0;
        }

        public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity is null)
            {
                return false;
            }

            DbSet.Remove(entity);
            return await Context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}

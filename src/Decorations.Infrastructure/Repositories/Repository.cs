using System.Linq.Expressions;
using Decorations.Application.Interfaces;
using Decorations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Decorations.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext context;

        public Repository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await this.context.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await this.context.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await this.context.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            this.context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            this.context.Set<T>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}

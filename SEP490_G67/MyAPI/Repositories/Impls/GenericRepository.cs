using Microsoft.EntityFrameworkCore;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq.Expressions;

namespace MyAPI.Repositories.Impls
{
    public abstract class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly SEP490_G67Context _context;
        protected GenericRepository(SEP490_G67Context context)
        {
            _context = context;
        }

        public async Task<T> Add(T entity)
        {
            _context.Add(entity);
            await _context.SaveChangesAsync();  
            return entity;
        }

        public async Task<T> Delete(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AsQueryable().Where(predicate).ToListAsync();
        }

        public async Task<T> Get(int id)
        {
            return await _context.FindAsync<T>(id);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task SaveChange()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<T> Update(T entity)
        {
             _context.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}

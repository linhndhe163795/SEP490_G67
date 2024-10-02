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

        public T Add(T entity)
        {
            return _context.Add(entity).Entity;
        }

        public T Delete(T entity)
        {
            return _context.Remove(entity).Entity;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().AsQueryable().Where(predicate).ToList();
        }

        public T Get(int id)
        {
            return _context.Find<T>(id);
        }

        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public void SaveChange()
        {
            _context.SaveChanges();
        }

        public T Update(T entity)
        {
            return _context.Update(entity)
               .Entity;
        }
    }
}

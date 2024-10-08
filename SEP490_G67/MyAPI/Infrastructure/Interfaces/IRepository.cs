using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRepository<T>
    {
        Task<T> Add (T entity);
        Task<T> Delete (T entity);
        Task<T> Get (int id);
        Task<T> Update (T entity);
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
        Task SaveChange();
    }
}

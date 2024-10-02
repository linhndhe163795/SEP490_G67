using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRepository<T>
    {
        T Add (T entity);
        T Delete (T entity);
        T Get (int id);
        T Update (T entity);
        IEnumerable<T> GetAll ();   
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        void SaveChange();
    }
}

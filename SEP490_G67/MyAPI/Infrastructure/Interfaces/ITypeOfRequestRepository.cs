using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

public interface ITypeOfRequestRepository : IRepository<TypeOfRequest>
{
    Task<IEnumerable<TypeOfRequest>> GetAll();
}

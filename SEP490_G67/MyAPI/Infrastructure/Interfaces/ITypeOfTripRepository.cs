using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

public interface ITypeOfTripRepository : IRepository<TypeOfTrip>
{
    Task<IEnumerable<TypeOfTrip>> GetAll();
}

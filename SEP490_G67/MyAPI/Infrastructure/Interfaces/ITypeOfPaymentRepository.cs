using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

public interface ITypeOfPaymentRepository : IRepository<TypeOfPayment>
{
    Task<IEnumerable<TypeOfPayment>> GetAll();
}

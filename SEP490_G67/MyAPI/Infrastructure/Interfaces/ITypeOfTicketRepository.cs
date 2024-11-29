using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

public interface ITypeOfTicketRepository : IRepository<TypeOfTicket>
{
    Task<IEnumerable<TypeOfTicket>> GetAll();
}

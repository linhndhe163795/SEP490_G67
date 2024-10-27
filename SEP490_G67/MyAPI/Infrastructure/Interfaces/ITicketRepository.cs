using MyAPI.DTOs.TicketDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task CreateTicketByUser(int? promotionId, int tripDetailsId, TicketDTOs ticketDTOs, int userId);
    }
}

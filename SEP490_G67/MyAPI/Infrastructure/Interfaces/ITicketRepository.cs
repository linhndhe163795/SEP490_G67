using MyAPI.DTOs.TicketDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task CreateTicketByUser(string? promotionCode, int tripDetailsId, TicketDTOs ticketDTOs, int userId);

        Task<List<ListTicketDTOs>> getAllTicket();

        Task<List<TicketDTOs>> getTicketAfterInputPromotion(string promotionCode);
        
    }
}

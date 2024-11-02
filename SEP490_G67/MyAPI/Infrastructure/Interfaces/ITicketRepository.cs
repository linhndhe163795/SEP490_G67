using MyAPI.DTOs.TicketDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task CreateTicketByUser(string? promotionCode, int tripDetailsId, TicketDTOs ticketDTOs, int userId);
        Task CreatTicketFromDriver(int vehicleId,int priceTrip, TicketFromDriverDTOs ticket, int driverId);
        Task<List<ListTicketDTOs>> getAllTicket();
        Task<int> GetPriceFromPoint(TicketFromDriverDTOs ticket, int vehicleId);
        Task<List<TicketNotPaid>> GetListTicketNotPaid (int vehicleId);
        Task CreateTicketForRentCar(int vehicleId, decimal price, TicketForRentCarDTO ticketRentalDTO,int userId);
    }
}

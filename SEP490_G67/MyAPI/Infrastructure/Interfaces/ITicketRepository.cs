using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<int> CreateTicketByUser(string? promotionCode, int tripDetailsId, BookTicketDTOs ticketDTOs, int userId, int numberTicket,DateTime dateTicket);
        Task CreatTicketFromDriver(int vehicleId,int priceTrip, TicketFromDriverDTOs ticket, int driverId, int numberTicket);
        Task<List<ListTicketDTOs>> getAllTicket();
        Task<int> GetPriceFromPoint(TicketFromDriverDTOs ticket, int vehicleId);
        Task<TicketNotPaidSummary> GetListTicketNotPaid (int vehicleId);
        Task UpdateStatusTicketNotPaid(int id, int driverId);
        Task<bool> UpdateStatusTicketForPayment(int id);
        Task AcceptOrDenyRequestRentCar(AddTicketForRentCarDTO addTicketForRentCarDTO);
        Task<TicketByIdDTOs> getTicketDetailsById(int ticketId, int userId);
        Task<RevenueTicketDTO> getRevenueTicket(int userId);
        Task<bool> deleteTicketTimeOut(int ticketId);
        Task<bool> UpdateVehicleInRequestAsync(int vehicleId, int requestId);
        Task<IEnumerable<VehicleBasicDto>> GetVehiclesByRequestIdAsync(int requestId);
        Task<List<ListTicketDTOs>> GetTicketByUserId(int userId);
        Task updateTicketByTicketId(int ticketId, int userId, TicketUpdateDTOs ticket);
        Task deleteTicketByTicketId(int id, int userId);
    }
}

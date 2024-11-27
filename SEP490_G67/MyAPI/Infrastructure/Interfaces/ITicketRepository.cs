using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<int> CreateTicketByUser(string? promotionCode, int tripDetailsId, BookTicketDTOs ticketDTOs, int userId, int numberTicket);
        Task CreatTicketFromDriver(int vehicleId,int priceTrip, TicketFromDriverDTOs ticket, int driverId, int numberTicket);
        Task<List<ListTicketDTOs>> getAllTicket();
        Task<int> GetPriceFromPoint(TicketFromDriverDTOs ticket, int vehicleId);
        Task<List<TicketNotPaid>> GetListTicketNotPaid (int vehicleId);
        Task UpdateStatusTicketNotPaid(int id, int driverId);

        Task<bool> UpdateStatusTicketForPayment(int id);

        Task AcceptOrDenyRequestRentCar(int requestId, bool choose, int vehicleId, decimal price);
        Task<TicketByIdDTOs> getTicketById(int ticketId);
        Task<RevenueTicketDTO> getRevenueTicket(DateTime startTime, DateTime endTime, int? vehicle,int? vehicleOwner, int userId);

        Task<bool> deleteTicketTimeOut(int ticketId);

        Task<bool> UpdateVehicleInRequestAsync(int vehicleId, int requestId);
        Task<IEnumerable<VehicleBasicDto>> GetVehiclesByRequestIdAsync(int requestId);

    }
}

using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPaymentRentVehicleRepository : IRepository<PaymentRentVehicle>
    {
        Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDate(DateTime startDate, DateTime endDate, int? CarOwnerId, int? vehicleId);
    }
}

using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPaymentRentVehicleRepository : IRepository<PaymentRentVehicle>
    {
        Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDate(int userId);
        //update revenue rent vehicle
        Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDateUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId, int userId);
    }
}

using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPaymentRentVehicleRepository : IRepository<PaymentRentVehicle>
    {
        Task<TotalPaymentRentVehicleDTO> getPaymentRentVehicleByDate(int userId);
    }
}

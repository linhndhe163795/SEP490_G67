using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.DTOs.PaymentRentDriver;
using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.DTOs.TicketDTOs;

namespace MyAPI.DTOs.RevenueDTOs
{
    public class RevenueDTO
    {
        public List<RevenueTicketDTO> revenueTicketDTOs { get; set; } = new List<RevenueTicketDTO>();
        public List<TotalPaymentRentVehicleDTO> totalPaymentRentVehicleDTOs { get; set; } = new List<TotalPaymentRentVehicleDTO>();
        public List<TotalLossCost> totalLossCosts { get; set; } = new List<TotalLossCost>();
        public List<TotalPayementRentDriver> totalPayementRentDrivers { get; set; } = new List<TotalPayementRentDriver>();
        public decimal? totalRevenue { get; set; }
    }
}

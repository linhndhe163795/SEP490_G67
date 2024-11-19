using MyAPI.DTOs.PaymentRentVehicle;

namespace MyAPI.DTOs.PaymentRentDriver
{
    public class TotalPayementRentDriver
    {
        public List<PaymentRentDriverDTO> PaymentRentDriverDTOs { get; set; } = new List<PaymentRentDriverDTO>();
        public decimal? Total { get; set; }
    }
}

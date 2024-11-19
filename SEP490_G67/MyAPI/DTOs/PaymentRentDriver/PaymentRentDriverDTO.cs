namespace MyAPI.DTOs.PaymentRentDriver
{
    public class PaymentRentDriverDTO
    {
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

namespace MyAPI.DTOs.PaymentRentDriver
{
    public class PaymentRentDriverDTO
    {
        public int? DriverId { get; set; }
        public string? DriverName{ get; set; }
        public int? vehicleId { get; set; }
        public string? LicenseVehicle { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

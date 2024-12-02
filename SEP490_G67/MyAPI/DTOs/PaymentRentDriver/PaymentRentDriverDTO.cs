namespace MyAPI.DTOs.PaymentRentDriver
{
    public class PaymentRentDriverDTO
    {
        public string? DriverName{ get; set; }
        public string? LicenseVehicle { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

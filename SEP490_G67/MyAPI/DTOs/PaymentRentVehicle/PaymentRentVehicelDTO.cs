namespace MyAPI.DTOs.PaymentRentVehicle
{
    public class PaymentRentVehicelDTO
    {
        public string? DriverName { get; set; }
        public string? LicenseVehicle {  get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CarOwner { get; set; }
    }
}

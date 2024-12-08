namespace MyAPI.DTOs.PaymentRentVehicle
{
    public class PaymentRentVehicelDTO
    {
        public int Id { get; set; }
        public int? driverId { get; set; }
        public string? DriverName { get; set; }
        public int? vehicelId { get; set; }
        public string? LicenseVehicle {  get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CarOwner { get; set; }
    }
}

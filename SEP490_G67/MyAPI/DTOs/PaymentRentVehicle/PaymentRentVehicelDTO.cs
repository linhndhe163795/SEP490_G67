namespace MyAPI.DTOs.PaymentRentVehicle
{
    public class PaymentRentVehicelDTO
    {
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CarOwnerId { get; set; }
    }
}

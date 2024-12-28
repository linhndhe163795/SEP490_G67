namespace MyAPI.DTOs.DriverDTOs
{
    public class DriverNotVehicleResponse
    {
        public int Id { get; set; }
        public int? vehicleId { get; set; }
        public string? userName { get; set; }
        public string? fullName { get; set; }
    }
}

namespace MyAPI.DTOs.RequestDTOs
{
    public class RequestDetailForRentDriver
    {
        public string VehicleId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; }
        public int? Price { get; set; }
    }
}


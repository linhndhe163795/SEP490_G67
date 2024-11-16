namespace MyAPI.DTOs.HistoryRentDriverDTOs
{
    public class RequestDetailForRentDriver
    {
        public int? DriverId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; }
        public int? Price { get; set; }
    }
}


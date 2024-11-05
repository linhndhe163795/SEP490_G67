namespace MyAPI.DTOs.RequestDTOs
{
    public class RequestDetailForRentCarDTO
    {
        public int? RequestId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; }
    }
}

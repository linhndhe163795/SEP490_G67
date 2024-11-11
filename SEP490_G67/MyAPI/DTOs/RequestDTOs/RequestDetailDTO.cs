namespace MyAPI.DTOs.RequestDTOs
{
    public class RequestDetailDTO
    {
        public int? RequestId { get; set; }

        public int? TicketId { get; set; }
        public int? VehicleId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public int? Price { get; set; }
    }
}

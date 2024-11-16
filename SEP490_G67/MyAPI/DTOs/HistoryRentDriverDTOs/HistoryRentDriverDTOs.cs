namespace MyAPI.DTOs.HistoryRentDriverDTOs
{
    public class HistoryRentDriverDTOs
    {
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? EndStart { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

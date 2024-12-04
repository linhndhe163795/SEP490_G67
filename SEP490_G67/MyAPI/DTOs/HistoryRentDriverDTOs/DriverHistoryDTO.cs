namespace MyAPI.DTOs.HistoryRentDriverDTOs
{
    public class DriverHistoryDTO
    {
        public int HistoryId { get; set; }
        public int? DriverId {  get; set; }
        public string DriverName { get; set; }
        public string LicensePlate { get; set; }
        public int vehicleOwnerId { get; set; }
        public string vehicleOwner { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? EndStart { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}

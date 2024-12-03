namespace MyAPI.DTOs.HistoryRentDriverDTOs
{
    public class DriverRentInfoDTO
    {
        public int HistoryId { get; set; }
        public string DriverName { get; set; }
        public string VehicleLicense { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? EndStart { get; set; }
        public decimal? Price { get; set; }
    }

}

namespace MyAPI.DTOs.HistoryRentDriverDTOs
{
    public class AddHistoryRentDriver
    {
        public int requestId { get; set; }
        public bool choose { get; set; }
        public int? driverId { get; set; }
        public decimal price { get; set; }
    }
}

namespace MyAPI.DTOs.HistoryRentVehicle
{
    public class RentVehicleAddDTO
    {
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public int? Seats { get; set; }
        public decimal? Price { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

    }
}

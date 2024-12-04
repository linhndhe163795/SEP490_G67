namespace MyAPI.DTOs.HistoryRentVehicles
{
    public class HistoryVehicleRentDTO
    {
        public int Id { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; }
        public int? VehicleId { get; set; }
        public string VehicleOwner { get; set; }
        public int? OwnerId { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? EndStart { get; set; }
    }
}

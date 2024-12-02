namespace MyAPI.DTOs.HistoryRentVehicleDTOs
{
    public class HistoryRentVehicleListDTO
    {
        public int Id { get; set; }
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public string? Image { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
    }
}

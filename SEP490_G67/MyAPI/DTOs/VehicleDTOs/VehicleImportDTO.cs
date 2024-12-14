namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleImportDTO
    {
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
        public bool? Flag { get; set; }
        public bool? Status { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

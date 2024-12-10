namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleAddDTO
    {
        public int? NumberSeat { get; set; }
        public string? vehicleTypeName { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public string? Image { get; set; }
        public string? driverName { get; set; }
        public int? driverId { get; set; }
        public string? vehicleOwnerName { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
    }
}

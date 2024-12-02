namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleAddDTO
    {
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public string? Image { get; set; }
        public string? driverName { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
    }
}

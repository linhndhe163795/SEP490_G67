namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleConvenienceRentResponseDTO
    {
        public int Id { get; set; }
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? VehicleOwnerName{ get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
    }
}

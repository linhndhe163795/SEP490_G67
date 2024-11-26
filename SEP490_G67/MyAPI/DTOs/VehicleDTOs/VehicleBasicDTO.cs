namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleBasicDto
    {
        public int? Id { get; set; }
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
    }

}

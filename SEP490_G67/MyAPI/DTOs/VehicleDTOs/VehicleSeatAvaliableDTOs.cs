namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleSeatAvaliableDTOs
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public int? NumberAvaliable { get; set; }
        public int? NumberInvaliable { get; set; }
        
    }
}

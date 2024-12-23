namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleSeatAvaliableDTOs
    {
        public int Id { get; set; }
        public TimeSpan? dateTime {get;set;}
        public string? description {get;set;}
        public int? tripId {get;set;}
        public int? NumberSeat { get; set; }
        public string? DriverId { get; set; }
        public string? LicensePlate { get; set; }
        public int? NumberAvaliable { get; set; }
        public int? NumberInvaliable { get; set; }
        
    }
}

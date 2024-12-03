namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleLicenscePlateDTOs
    {
        public int Id { get; set; }
        public string LicensePlate{ get; set; }
        public int? NumberSeat { get; set; }
        public string VehicleTypeName { get; set; }
        public string DriverName { get; set; }
        public bool? Status { get; set; }
        public string Description { get; set; }
    }
}

namespace MyAPI.DTOs.VehicleTripDTOs
{
    public class VehicleTripDTO
    {
        public int TripId { get; set; }
        public int VehicleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

    }
}

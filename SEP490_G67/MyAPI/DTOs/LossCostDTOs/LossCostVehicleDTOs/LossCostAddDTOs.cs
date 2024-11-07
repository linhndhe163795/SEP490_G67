namespace MyAPI.DTOs.LossCostDTOs.LossCostVehicleDTOs
{
    public class LossCostAddDTOs
    {
        public int? VehicleId { get; set; }
        public int? LossCostTypeId { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public DateTime? DateIncurred { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}

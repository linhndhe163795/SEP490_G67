using System.Text.Json.Serialization;

namespace MyAPI.DTOs.LossCostDTOs.LossCostVehicleDTOs
{
    public class LossCostUpdateDTO
    {
        public int? VehicleId { get; set; }
        public int? LossCostTypeId { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public DateTime? DateIncurred { get; set; }
        [JsonIgnore]
        public DateTime? UpdateAt { get; set; }
      
    }
}

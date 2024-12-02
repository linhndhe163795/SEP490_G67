using System.Text.Json.Serialization;

namespace MyAPI.DTOs.TripDTOs
{
    public class UpdateTrip
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public bool? Status { get; set; }
        public int? TypeOfTrip { get; set; }
        [JsonIgnore]
        public DateTime? CreatedAt { get; set; }
        [JsonIgnore]
        public int? CreatedBy { get; set; }
  
    }
}

using MyAPI.Models;

namespace MyAPI.DTOs.TripDTOs
{
    public class TripDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public TimeSpan? StartTime{ get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
       
    }
}

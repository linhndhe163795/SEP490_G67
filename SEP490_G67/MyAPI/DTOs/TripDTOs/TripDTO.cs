//using MyAPI.Models;

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyAPI.DTOs.TripDTOs
{
    public class TripDTO
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
        public string LicensePlate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public Dictionary<string, TimeSpan> PointStartDetail { get; set; } = new Dictionary<string, TimeSpan>();
        public Dictionary<string, TimeSpan> PointEndDetail { get; set; } = new Dictionary<string, TimeSpan>();

    }

}

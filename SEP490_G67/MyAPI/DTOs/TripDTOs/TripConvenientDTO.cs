namespace MyAPI.DTOs.TripDTOs
{
    public class TripConvenientDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public bool? Status { get; set; }
        public int TypeOfTrip { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
       
    }
}

namespace MyAPI.DTOs.TripDTOs
{
    public class TripDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartTime{ get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

namespace MyAPI.DTOs.ReviewDTOs
{
    public class ReviewDTO
    {
        public string? Description { get; set; }
        public int? TripId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

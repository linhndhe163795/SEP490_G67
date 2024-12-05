namespace MyAPI.DTOs.PointUserDTOs
{
    public class PointHistoryDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? Points { get; set; }
        public int? MinusPoints { get; set; }
        public DateTime? Date { get; set; }
    }
}

namespace MyAPI.DTOs.PromotionUserDTOs
{
    public class ListPromotionUserDTO
    {
        public int Id { get; set; }
        public string CodePromotion { get; set; } = null!;
        public string? ImagePromotion { get; set; }
        public int Discount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; } = null!;
    }
}

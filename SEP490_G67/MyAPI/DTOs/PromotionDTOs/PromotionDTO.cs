namespace MyAPI.DTOs.PromotionDTOs
{
    public class PromotionDTO
    {
        public int Id { get; set; }
        public string CodePromotion { get; set; } = null!;
        public string? ImagePromotion { get; set; }
        public string? Description { get; set; };
        public int Discount { get; set; }
        public int? ExchangePoint { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

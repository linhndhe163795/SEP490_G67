namespace MyAPI.DTOs.PromotionDTOs
{
    public class PromotionDTO
    {
        //public int Id { get; set; }
        public string CodePromotion { get; set; } = null!;
        public string? ImagePromotion { get; set; }
        public string Description { get; set; } = null!;
        public int Discount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

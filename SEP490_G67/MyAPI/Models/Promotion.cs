using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Promotion
    {
        public Promotion()
        {
            PromotionUsers = new HashSet<PromotionUser>();
        }

        public int Id { get; set; }
        public string CodePromotion { get; set; } = null!;
        public string? ImagePromotion { get; set; }
        public int Discount { get; set; }
        public int? ExchangePoint { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<PromotionUser> PromotionUsers { get; set; }
    }
}

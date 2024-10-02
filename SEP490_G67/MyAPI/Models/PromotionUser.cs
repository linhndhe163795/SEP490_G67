using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class PromotionUser
    {
        public int UserId { get; set; }
        public int PromotionId { get; set; }
        public DateTime? DateReceived { get; set; }

        public virtual Promotion Promotion { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

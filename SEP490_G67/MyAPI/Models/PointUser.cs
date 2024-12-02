using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class PointUser
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? PaymentId { get; set; }
        public int? Points { get; set; }
        public int? PointsMinus { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Payment? Payment { get; set; }
        public virtual User? User { get; set; }
    }
}

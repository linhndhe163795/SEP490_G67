using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class StatusPayment
    {
        public StatusPayment()
        {
            Payments = new HashSet<Payment>();
        }

        public int Id { get; set; }
        public string? TypeOfPayment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}

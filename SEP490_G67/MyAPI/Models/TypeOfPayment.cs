using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class TypeOfPayment
    {
        public TypeOfPayment()
        {
            Payments = new HashSet<Payment>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string? TypeOfPayment1 { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}

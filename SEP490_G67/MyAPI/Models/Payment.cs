using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Payment
    {
        public Payment()
        {
            PointUsers = new HashSet<PointUser>();
            UserCancleTickets = new HashSet<UserCancleTicket>();
        }

        public int PaymentId { get; set; }
        public int? UserId { get; set; }
        public decimal? Price { get; set; }
        public string? Code { get; set; }
        public DateTime? Time { get; set; }
        public string? Description { get; set; }
        public int? TypeOfPayment { get; set; }
        public int? TicketId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Ticket? Ticket { get; set; }
        public virtual TypeOfPayment? TypeOfPaymentNavigation { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<PointUser> PointUsers { get; set; }
        public virtual ICollection<UserCancleTicket> UserCancleTickets { get; set; }
    }
}

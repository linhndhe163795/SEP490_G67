using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class User
    {
        public User()
        {
            Payments = new HashSet<Payment>();
            PointUsers = new HashSet<PointUser>();
            PromotionUsers = new HashSet<PromotionUser>();
            Reviews = new HashSet<Review>();
            Tickets = new HashSet<Ticket>();
            UserCancleTickets = new HashSet<UserCancleTicket>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? NumberPhone { get; set; }
        public string? Password { get; set; }
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public bool? Status { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<PointUser> PointUsers { get; set; }
        public virtual ICollection<PromotionUser> PromotionUsers { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<UserCancleTicket> UserCancleTickets { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}

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
            Requests = new HashSet<Request>();
            Reviews = new HashSet<Review>();
            Tickets = new HashSet<Ticket>();
            UserCancleTickets = new HashSet<UserCancleTicket>();
            UserRoles = new HashSet<UserRole>();
            Vehicles = new HashSet<Vehicle>();
        }

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string NumberPhone { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? ActiveCode { get; set; }
        public bool? Status { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<PointUser> PointUsers { get; set; }
        public virtual ICollection<PromotionUser> PromotionUsers { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<UserCancleTicket> UserCancleTickets { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }
}

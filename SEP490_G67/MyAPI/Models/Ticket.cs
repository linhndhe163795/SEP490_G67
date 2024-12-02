using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Ticket
    {
        public Ticket()
        {
            ChangeTimeTrips = new HashSet<ChangeTimeTrip>();
            Payments = new HashSet<Payment>();
            UserCancleTickets = new HashSet<UserCancleTicket>();
        }

        public int Id { get; set; }
        public decimal? Price { get; set; }
        public string? CodePromotion { get; set; }
        public decimal? PricePromotion { get; set; }
        public int? NumberTicket { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public int? UserId { get; set; }
        public int? VehicleId { get; set; }
        public int? TripId { get; set; }
        public int? TypeOfTicket { get; set; }
        public int? TypeOfPayment { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Trip? Trip { get; set; }
        public virtual TypeOfPayment? TypeOfPaymentNavigation { get; set; }
        public virtual TypeOfTicket? TypeOfTicketNavigation { get; set; }
        public virtual User? User { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ICollection<ChangeTimeTrip> ChangeTimeTrips { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<UserCancleTicket> UserCancleTickets { get; set; }
    }
}

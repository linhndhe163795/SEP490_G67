using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Trip
    {
        public Trip()
        {
            Reviews = new HashSet<Review>();
            Tickets = new HashSet<Ticket>();
            TripDetails = new HashSet<TripDetail>();
            VehicleTrips = new HashSet<VehicleTrip>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
        public int? TypeOfTrip { get; set; }

        public virtual TypeOfTrip? TypeOfTripNavigation { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<TripDetail> TripDetails { get; set; }
        public virtual ICollection<VehicleTrip> VehicleTrips { get; set; }
    }
}

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
            VehicleTrips = new HashSet<VehicleTrip>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartTime { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<VehicleTrip> VehicleTrips { get; set; }
    }
}

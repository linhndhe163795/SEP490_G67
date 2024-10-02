﻿using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Trip
    {
        public Trip()
        {
            Reviews = new HashSet<Review>();
            StopPoinTrips = new HashSet<StopPoinTrip>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Description { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public int? VehicleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<StopPoinTrip> StopPoinTrips { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}

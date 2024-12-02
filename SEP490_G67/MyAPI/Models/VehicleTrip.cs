using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class VehicleTrip
    {
        public int TripId { get; set; }
        public int VehicleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Trip Trip { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
    }
}

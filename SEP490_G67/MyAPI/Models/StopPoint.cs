using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class StopPoint
    {
        public StopPoint()
        {
            StopPoinTrips = new HashSet<StopPoinTrip>();
        }

        public int TripId { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<StopPoinTrip> StopPoinTrips { get; set; }
    }
}

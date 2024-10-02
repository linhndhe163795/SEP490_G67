using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class StopPoinTrip
    {
        public int TripId { get; set; }
        public int StopPointId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual StopPoint StopPoint { get; set; } = null!;
        public virtual Trip Trip { get; set; } = null!;
    }
}

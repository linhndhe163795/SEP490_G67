﻿using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class RequestDetail
    {
        public int DetailId { get; set; }
        public int? RequestId { get; set; }
        public int? VehicleId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; }

        public virtual Request? Request { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}

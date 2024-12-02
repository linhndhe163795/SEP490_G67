using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class VehicleSeatStatus
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public int? SeatNumber { get; set; }
        public string? Status { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
    }
}

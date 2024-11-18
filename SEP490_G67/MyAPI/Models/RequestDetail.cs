using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class RequestDetail
    {
        public int DetailId { get; set; }
        public int? RequestId { get; set; }
        public int? VehicleId { get; set; }
        public int? TicketId { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public int? DriverId { get; set; }

        public virtual Request? Request { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}

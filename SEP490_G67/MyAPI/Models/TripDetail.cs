using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class TripDetail
    {
        public int Id { get; set; }
        public string? PointStartDetails { get; set; }
        public string? PointEndDetails { get; set; }
        public TimeSpan? TimeStartDetils { get; set; }
        public TimeSpan? TimeEndDetails { get; set; }
        public int? TripId { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Trip? Trip { get; set; }
    }
}

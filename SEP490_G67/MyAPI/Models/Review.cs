using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Review
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int? UserId { get; set; }
        public int? TripId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Trip? Trip { get; set; }
        public virtual User? User { get; set; }
    }
}

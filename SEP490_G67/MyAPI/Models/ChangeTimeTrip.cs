using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class ChangeTimeTrip
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
        public DateTime? NewStartupdate { get; set; }
        public int? TickedId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Ticket? Ticked { get; set; }
    }
}

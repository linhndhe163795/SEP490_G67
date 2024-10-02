using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class LossCost
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public int? LossCostTypeId { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public DateTime? DateIncurred { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual LossCostType? LossCostType { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}

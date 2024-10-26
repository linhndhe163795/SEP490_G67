using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class LossCostType
    {
        public LossCostType()
        {
            LossCosts = new HashSet<LossCost>();
        }

        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<LossCost> LossCosts { get; set; }
    }
}

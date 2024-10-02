using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class TypeOfDriver
    {
        public TypeOfDriver()
        {
            Drivers = new HashSet<Driver>();
        }

        public int Id { get; set; }
        public string? Description { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual ICollection<Driver> Drivers { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class TypeOfTrip
    {
        public TypeOfTrip()
        {
            Trips = new HashSet<Trip>();
        }

        public int Id { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Trip> Trips { get; set; }
    }
}

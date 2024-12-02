using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class TypeOfRequest
    {
        public TypeOfRequest()
        {
            Requests = new HashSet<Request>();
        }

        public int Id { get; set; }
        public string TypeName { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Request> Requests { get; set; }
    }
}

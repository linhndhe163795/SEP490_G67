using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Request
    {
        public Request()
        {
            RequestDetails = new HashSet<RequestDetail>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TypeId { get; set; }
        public bool Status { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual TypeOfRequest? Type { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<RequestDetail> RequestDetails { get; set; }
    }
}

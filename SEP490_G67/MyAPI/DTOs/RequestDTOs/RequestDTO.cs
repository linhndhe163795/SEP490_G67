    namespace MyAPI.DTOs.RequestDTOs
    {
        public class RequestDTO
        {
            public int UserId { get; set; }
            public int? TypeId { get; set; }
            public bool Status { get; set; }
            public string? Description { get; set; }
            public string? Note { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int? CreatedBy { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public int? UpdatedBy { get; set; }
        public List<RequestDetailDTO> RequestDetails { get; set; } = new List<RequestDetailDTO>();
        }
    }

    namespace MyAPI.DTOs.RequestDTOs
    {
        public class RequestDTOForRentCar
        {
            public int? TypeId { get; set; }
            public bool Status { get; set; }
            public string? Description { get; set; }
            public string? Note { get; set; }
            public DateTime? CreatedAt { get; set; }
        public List<RequestDetailForRentCarDTO> RequestDetails { get; set; } = new List<RequestDetailForRentCarDTO>();
        }
    }

using System.Text.Json.Serialization;

namespace MyAPI.DTOs.RequestDTOs
{
    public class RequestCancleTicketDTOs
    {
        [JsonIgnore]
        public int? TypeId { get; set; }
        public string? Description { get; set; }
        public int? TicketId { get; set; }
        [JsonIgnore]
        public DateTime? CreatedAt { get; set; }
        [JsonIgnore]
        public int? CreatedBy { get; set; }
    }
}

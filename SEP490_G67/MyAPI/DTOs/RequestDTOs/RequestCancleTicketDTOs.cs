using System.Text.Json.Serialization;

namespace MyAPI.DTOs.RequestDTOs
{
    public class RequestCancleTicketDTOs
    {
        [JsonIgnore]
        public int? TypeId { get; set; }
        [JsonIgnore]
        public string? Description { get; set; }
        public int? TicketId { get; set; }
    }
}

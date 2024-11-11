using System.Text.Json.Serialization;

namespace MyAPI.DTOs.TicketDTOs
{
    public class BookTicketDTOs
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Note { get; set; }
        public int? TypeOfPayment { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace MyAPI.DTOs.TypeOfDTOs
{
    public class TypeOfTicketDTO
    {
        public int Id { get; set; }
        [JsonPropertyName("Name")]
        public string? Description { get; set; }
    }
}

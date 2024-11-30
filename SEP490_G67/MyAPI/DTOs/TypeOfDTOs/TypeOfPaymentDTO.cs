using System.Text.Json.Serialization;

namespace MyAPI.DTOs.TypeOfDTOs
{
    public class TypeOfPaymentDTO
    {
        public int Id { get; set; }
        [JsonPropertyName("Name")]
        public string? TypeOfPayment1 { get; set; }
    }
}

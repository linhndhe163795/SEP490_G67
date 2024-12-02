using System.Text.Json.Serialization;

namespace MyAPI.DTOs.TicketDTOs
{
    public class TicketDTOs
    {
        public decimal? Price { get; set; }
        public string? CodePromotion { get; set; }
        [JsonIgnore]
        public decimal? PricePromotion { get; set; }
        public int NumberTicket { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public int? UserId { get; set; }
        public int? VehicleId { get; set; }
        public int? TripId { get; set; }
        [JsonIgnore]
        public int? TypeOfTicket { get; set; }
        public int? TypeOfPayment { get; set; }
        [JsonIgnore]
        public string? Status { get; set; }
        [JsonIgnore]
        public DateTime? CreatedAt { get; set; }
        [JsonIgnore]
        public int? CreatedBy { get; set; }
        [JsonIgnore]
        public DateTime? UpdateAt { get; set; }
        [JsonIgnore]
        public int? UpdateBy { get; set; }
    }
}

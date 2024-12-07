namespace MyAPI.DTOs.TicketDTOs
{
    public class TicketByIdDTOs
    {
        public decimal? Price { get; set; }
        public string? CodePromotion { get; set; }
        public decimal? PricePromotion { get; set; }
        public string? SeatCode { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string? TypeOfPayment { get; set; }
        public string? LicsenceVehicle { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public int? UserId { get; set; }
        public string? fullName { get; set; }
        public int? VehicleId { get; set; }
        public int? TripId { get; set; }
        public string? Status { get; set; }
    }
}

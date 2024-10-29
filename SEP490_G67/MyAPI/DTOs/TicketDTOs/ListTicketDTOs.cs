namespace MyAPI.DTOs.TicketDTOs
{
    public class ListTicketDTOs
    {
        public decimal? Price { get; set; }
        public string? CodePromotion { get; set; }
        public decimal? PricePromotion { get; set; }
        public string? SeatCode { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public int? UserId { get; set; }
        public int? VehicleId { get; set; }
        public int? TripId { get; set; }
        public int? TypeOfTicket { get; set; }
        public int? TypeOfPayment { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

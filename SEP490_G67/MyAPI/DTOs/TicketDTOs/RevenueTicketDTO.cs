namespace MyAPI.DTOs.TicketDTOs
{
    public class RevenueTicketDTO
    {
        public List<TicketRevenue> listTicket { get; set; } = new List<TicketRevenue>();

        public decimal? total { get; set; }
    }

    public class TicketRevenue
    {
        public decimal? PricePromotion { get; set; }
        public int? VehicleId { get; set; }
        public int? TypeOfTicket { get; set; }
        public int? TypeOfPayment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

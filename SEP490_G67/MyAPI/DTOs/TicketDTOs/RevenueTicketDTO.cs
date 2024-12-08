namespace MyAPI.DTOs.TicketDTOs
{
    public class RevenueTicketDTO
    {
        public List<TicketRevenue> listTicket { get; set; } = new List<TicketRevenue>();

        public decimal? total { get; set; }
    }

    public class TicketRevenue
    {
        public int Id { get; set; }
        public decimal? PricePromotion { get; set; }
        public string? VehicleOwner { get; set; }
        public int? VehicleId { get; set; }
        public string LiscenseVehicle { get; set; }
        public string TypeOfTicket { get; set; }
        public string TypeOfPayment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}

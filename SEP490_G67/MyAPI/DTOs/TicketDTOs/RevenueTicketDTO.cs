namespace MyAPI.DTOs.TicketDTOs
{
    public class RevenueTicketDTO
    {
        public List<TicketDTOs> listTicket { get; set; } = new List<TicketDTOs>();

        public decimal? total { get; set; }
    }
}

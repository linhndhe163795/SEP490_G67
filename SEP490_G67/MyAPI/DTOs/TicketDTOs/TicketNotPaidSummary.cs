namespace MyAPI.DTOs.TicketDTOs
{
    public class TicketNotPaidSummary
    {
        public List<TicketNotPaid> Tickets { get; set; }
        public decimal Total { get; set; }
    }
}

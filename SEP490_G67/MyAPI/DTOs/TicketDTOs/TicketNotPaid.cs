namespace MyAPI.DTOs.TicketDTOs
{
    public class TicketNotPaid
    {
        public int? ticketId { get; set; }
        public int? userId { get; set; }
        public string FullName { get; set; }
        public decimal Price { get; set; }
        public string TypeOfPayment { get; set; }
        public decimal total {  get; set; }
    }
}

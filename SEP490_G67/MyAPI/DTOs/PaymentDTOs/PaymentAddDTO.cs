namespace MyAPI.DTOs.PaymentDTOs
{
    public class PaymentAddDTO
    {
        public decimal? Price { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TypeOfPayment { get; set; }
        public int? TicketId { get; set; }
    }
}

namespace MyAPI.DTOs.PaymentDTOs
{
    public class PaymentAddDTO
    {
        public int? UserId { get; set; }
        public decimal? Price { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TypeOfPayment { get; set; }
        public int? TicketId { get; set; }

        public DateTime? Time { get; set; }
    }
}

namespace MyAPI.DTOs.TicketDTOs
{
    public class AddTicketForRentCarDTO
    {
        public int requestId { get; set; }
        public bool choose { get; set; }
        public int vehicleId { get; set; }
        public decimal price { get; set; }
    }
}

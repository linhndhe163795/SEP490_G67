namespace MyAPI.DTOs.UserCancleTicketDTOs
{
    public class AddUserCancleTicketDTOs
    {
       
        public string? ReasonCancle { get; set; }
        public int? TicketId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}

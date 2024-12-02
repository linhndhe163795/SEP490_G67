namespace MyAPI.DTOs.PointUserDTOs
{
    public class PointUserAddDTO
    {
        public int? UserId { get; set; }
        public int? PaymentId { get; set; }
        public int? Points { get; set; }
        public int? PointsMinus { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdateBy { get; set; }
    }
}

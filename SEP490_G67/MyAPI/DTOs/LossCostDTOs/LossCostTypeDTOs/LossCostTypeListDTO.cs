namespace MyAPI.DTOs.LossCostDTOs.LossCostTypeDTOs
{
    public class LossCostTypeListDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

namespace MyAPI.DTOs.RoleDTOs
{
    public class RoleAddDTO
    {
        public string? RoleName { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

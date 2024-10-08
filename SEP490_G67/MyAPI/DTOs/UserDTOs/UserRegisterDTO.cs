namespace MyAPI.DTOs.UserDTOs
{
    public class UserRegisterDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? NumberPhone { get; set; }
        public string? Password { get; set; }
        public DateTime? Dob { get; set; }
        public bool? Status { get; set; }
        public string? ActiveCode { get; set; }
        public DateTime? UpdateAt { get; set; } 
        public DateTime? CreatedAt { get; set; }

    }
}

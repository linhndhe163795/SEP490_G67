namespace MyAPI.DTOs.UserDTOs
{
    public class UserPostLoginDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;
        public string NumberPhone { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? ActiveCode { get; set; }
        public bool? Status { get; set; }
        public DateTime? Dob { get; set; }
    }
}

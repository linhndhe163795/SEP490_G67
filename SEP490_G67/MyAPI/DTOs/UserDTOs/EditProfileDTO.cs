namespace MyAPI.DTOs.UserDTOs
{
    public class EditProfileDTO
    {
        public string Email { get; set; } = null!;
        public string NumberPhone { get; set; } = null!;
        public string? Avatar { get; set; }
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? Dob { get; set; }
    }

}

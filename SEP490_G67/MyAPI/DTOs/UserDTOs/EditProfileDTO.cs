namespace MyAPI.DTOs.UserDTOs
{
    public class EditProfileDTO
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string NumberPhone { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? Dob { get; set; }
    }

}

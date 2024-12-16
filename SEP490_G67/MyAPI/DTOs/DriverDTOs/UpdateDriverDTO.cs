namespace MyAPI.DTOs.DriverDTOs
{
    public class UpdateDriverDTO
    {
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? NumberPhone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? License { get; set; }
        public string? Avatar { get; set; }
        public DateTime? Dob { get; set; }
        public bool? Status { get; set; }
    }
}

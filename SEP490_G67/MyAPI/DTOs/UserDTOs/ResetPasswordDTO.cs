namespace MyAPI.DTOs.UserDTOs
{
    public class ResetPasswordDTO
    {
        public string? Email { get; set; }
        public string Code { get; set; }
        public string? Password { get; set; }    
        public string? ConfirmPassword { get; set; }

    }
}

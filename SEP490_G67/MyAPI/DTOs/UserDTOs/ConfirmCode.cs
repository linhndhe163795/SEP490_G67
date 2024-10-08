namespace MyAPI.DTOs
{
    public class ConfirmCode
    {
        public string? Email { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}

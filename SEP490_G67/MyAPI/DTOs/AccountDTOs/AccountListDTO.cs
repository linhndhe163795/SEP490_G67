namespace MyAPI.DTOs.AccountDTOs
{
    public class AccountListDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? NumberPhone { get; set; }
        public string? Password { get; set; }
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public bool? Status { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }
    }
}

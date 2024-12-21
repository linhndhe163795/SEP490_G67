namespace MyAPI.DTOs.UserDTOs
{
    public class CreateAccountDTO
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string NumberPhone { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }
}

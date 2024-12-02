namespace MyAPI.DTOs.UserDTOs
{
        public class ChangePasswordDTO
        {
            public string CurrentEmail { get; set; }
            public string NewPassword { get; set; }     
            public string OldPassword { get; set; }     
        }
}

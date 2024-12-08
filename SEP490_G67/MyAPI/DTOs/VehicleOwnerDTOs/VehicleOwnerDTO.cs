namespace MyAPI.DTOs.VehicleOwnerDTOs
{
    public class VehicleOwnerDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string NumberPhone { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public bool? Status { get; set; }
        public DateTime? Dob { get; set; }
    }
}

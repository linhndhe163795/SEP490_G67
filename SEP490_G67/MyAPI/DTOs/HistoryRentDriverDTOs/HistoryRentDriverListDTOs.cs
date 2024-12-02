namespace MyAPI.DTOs.HistoryRentDriverDTOs
{
    public class HistoryRentDriverListDTOs
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string NumberPhone { get; set; }
        public string License { get; set; }
        public string Avatar { get; set; }
        public DateTime? Dob { get; set; }
        public string StatusWork { get; set; }
        public int? TypeOfDriver { get; set; }
        public bool? Status { get; set; }
        public string Email { get; set; }
    }
}

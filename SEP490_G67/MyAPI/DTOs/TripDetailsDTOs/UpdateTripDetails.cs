namespace MyAPI.DTOs.TripDetailsDTOs
{
    public class UpdateTripDetails
    {
        public string? PointStartDetails { get; set; }
        public string? PointEndDetails { get; set; }
        public TimeSpan? TimeStartDetils { get; set; }
        public TimeSpan? TimeEndDetails { get; set; }
        public bool? Status { get; set; }
    }
}

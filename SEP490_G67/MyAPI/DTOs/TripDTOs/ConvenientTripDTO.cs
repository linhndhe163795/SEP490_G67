﻿namespace MyAPI.DTOs.TripDTOs
{
    public class ConvenientTripDTO
    {
        public string? UserName { get; set; }
        public DateTime? StartTime { get; set; }
        public decimal? Price { get; set; }
        public int? SeatNumber { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PromotionCode { get; set; }
        public int? TypeOfTrip { get; set; }
    }
}

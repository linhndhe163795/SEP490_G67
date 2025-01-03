﻿namespace MyAPI.DTOs.RequestDTOs
{
    public class RequestDetailDTO
    {
        public int? RequestId { get; set; }
        public int? TicketId { get; set; }
        public int? VehicleId { get; set; }
        public bool? Status { get; set; }
        public string? LicensePlate { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? Note { get; set; }
        public int? typeRequestId { get; set; }
        public string? typeName { get; set; }
        public string? phoneNumber { get; set; }
        public string? promotionCode { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Seats { get; set; } 
        public string? UserName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public decimal? Price { get; set; }
    }
}

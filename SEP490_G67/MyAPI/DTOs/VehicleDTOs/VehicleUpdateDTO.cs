﻿namespace MyAPI.DTOs.VehicleDTOs
{
    public class VehicleUpdateDTO
    {
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public string? Image { get; set; }
        public int? DriverId { get; set; }
        public bool? Flag { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
    }
}

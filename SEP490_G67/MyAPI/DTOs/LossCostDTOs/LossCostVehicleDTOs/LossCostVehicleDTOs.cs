﻿namespace MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs
{
    public class AddLostCostVehicleDTOs
    {
        public int? VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public string? LossCostType { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public int? VehicleOwnerId { get; set; }
        public string? VehicleOwner { get; set; }
        public DateTime? DateIncurred { get; set; }

    }
    public class TotalLossCost
    {
        public List<AddLostCostVehicleDTOs> listLossCostVehicle { get; set; } = new List<AddLostCostVehicleDTOs>();
        public decimal? TotalCost { get; set; }
    }
}

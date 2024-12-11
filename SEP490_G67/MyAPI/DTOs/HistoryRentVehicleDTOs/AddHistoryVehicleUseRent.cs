namespace MyAPI.DTOs.HistoryRentVehicleDTOs
{
    public class AddHistoryVehicleUseRent
    {
        //int requestId, bool choose, int? vehicleId, decimal price   
        public int requestId { get; set; }
        public bool choose { get; set; }
        public int? vehicleId { get; set; }
        public decimal? price { get; set; }
    }
}

using MyAPI.DTOs.VehicleDTOs;

namespace MyAPI.DTOs.TripDTOs
{
    public class TripVehicleDTO
    {
        public TimeSpan? StartTime { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public List<VehicleDTO> listVehicle { get; set; } = new List<VehicleDTO>();
    }
}

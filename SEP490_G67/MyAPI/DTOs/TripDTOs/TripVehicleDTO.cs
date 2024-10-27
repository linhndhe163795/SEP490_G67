using MyAPI.DTOs.VehicleDTOs;

namespace MyAPI.DTOs.TripDTOs
{
    public class TripVehicleDTO
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public string? Description { get; set; }
        public string? PointStart { get; set; }
        public string? PointEnd { get; set; }
        public List<VehicleDTO> listVehicle { get; set; } = new List<VehicleDTO>();
    }
}

using MyAPI.Models;

namespace MyAPI.DTOs.DriverDTOs
{
    public class HistoryRentDriver
    {
        public int HistoryId { get; set; }           
        public int DriverId { get; set; }           
        public int VehicleId { get; set; }           
        public DateTime TimeStart { get; set; }
        public DateTime EndStart { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }

        public Driver Driver { get; set; }
        public Vehicle Vehicle { get; set; }
        public ICollection<PaymentRentDriver> PaymentRentDrivers { get; set; }
    }

}

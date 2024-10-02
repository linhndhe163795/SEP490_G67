using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class HistoryRentVehicle
    {
        public HistoryRentVehicle()
        {
            PaymentRentVehicles = new HashSet<PaymentRentVehicle>();
        }

        public int HistoryId { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public int? OwnerId { get; set; }
        public DateTime? TimeStart { get; set; }
        public DateTime? EndStart { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Driver? Driver { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ICollection<PaymentRentVehicle> PaymentRentVehicles { get; set; }
    }
}

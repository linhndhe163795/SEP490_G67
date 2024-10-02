using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class PaymentRentVehicle
    {
        public int Id { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public int? CarOwnerId { get; set; }
        public decimal? Price { get; set; }
        public int? HistoryRentVehicleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual HistoryRentVehicle? HistoryRentVehicle { get; set; }
    }
}

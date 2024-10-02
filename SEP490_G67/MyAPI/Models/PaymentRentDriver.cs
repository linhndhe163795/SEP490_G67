using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class PaymentRentDriver
    {
        public int Id { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleId { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? HistoryRentDriverId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual HistoryRentDriver? HistoryRentDriver { get; set; }
    }
}

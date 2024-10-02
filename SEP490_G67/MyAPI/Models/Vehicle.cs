using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Vehicle
    {
        public Vehicle()
        {
            HistoryRentDrivers = new HashSet<HistoryRentDriver>();
            HistoryRentVehicles = new HashSet<HistoryRentVehicle>();
            LossCosts = new HashSet<LossCost>();
            Tickets = new HashSet<Ticket>();
            Trips = new HashSet<Trip>();
        }

        public int Id { get; set; }
        public int? Seat { get; set; }
        public int? CarTypeId { get; set; }
        public bool? Status { get; set; }
        public int? DriverId { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual VehicleType? CarType { get; set; }
        public virtual Driver? Driver { get; set; }
        public virtual ICollection<HistoryRentDriver> HistoryRentDrivers { get; set; }
        public virtual ICollection<HistoryRentVehicle> HistoryRentVehicles { get; set; }
        public virtual ICollection<LossCost> LossCosts { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Trip> Trips { get; set; }
    }
}

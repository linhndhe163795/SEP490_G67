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
            RequestDetails = new HashSet<RequestDetail>();
            Tickets = new HashSet<Ticket>();
            VehicleSeatStatuses = new HashSet<VehicleSeatStatus>();
            VehicleTrips = new HashSet<VehicleTrip>();
        }

        public int Id { get; set; }
        public int? NumberSeat { get; set; }
        public int? VehicleTypeId { get; set; }
        public bool? Status { get; set; }
        public string? Image { get; set; }
        public int? DriverId { get; set; }
        public int? VehicleOwner { get; set; }
        public string? LicensePlate { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual Driver? Driver { get; set; }
        public virtual User? VehicleOwnerNavigation { get; set; }
        public virtual VehicleType? VehicleType { get; set; }
        public virtual ICollection<HistoryRentDriver> HistoryRentDrivers { get; set; }
        public virtual ICollection<HistoryRentVehicle> HistoryRentVehicles { get; set; }
        public virtual ICollection<LossCost> LossCosts { get; set; }
        public virtual ICollection<RequestDetail> RequestDetails { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<VehicleSeatStatus> VehicleSeatStatuses { get; set; }
        public virtual ICollection<VehicleTrip> VehicleTrips { get; set; }
    }
}

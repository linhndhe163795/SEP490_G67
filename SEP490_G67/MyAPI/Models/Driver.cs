using System;
using System.Collections.Generic;

namespace MyAPI.Models
{
    public partial class Driver
    {
        public Driver()
        {
            HistoryRentDrivers = new HashSet<HistoryRentDriver>();
            HistoryRentVehicles = new HashSet<HistoryRentVehicle>();
            Vehicles = new HashSet<Vehicle>();
        }

        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? NumberPhone { get; set; }
        public string? License { get; set; }
        public string? Avatar { get; set; }
        public DateTime? Dob { get; set; }
        public string? StatusWork { get; set; }
        public int? TypeOfDriver { get; set; }
        public bool? Status { get; set; }
        public string? Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int? UpdateBy { get; set; }

        public virtual TypeOfDriver? TypeOfDriverNavigation { get; set; }
        public virtual ICollection<HistoryRentDriver> HistoryRentDrivers { get; set; }
        public virtual ICollection<HistoryRentVehicle> HistoryRentVehicles { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }
}

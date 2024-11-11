namespace MyAPI.DTOs.DriverDTOs
{
    public class RentDriverDTO
    {
        public int DriverId { get; set; }           
        public int VehicleId { get; set; }         
        public DateTime TimeStart { get; set; }     
        public DateTime EndStart { get; set; }      
        public decimal Price { get; set; }          
        public string Description { get; set; }     
        public string CreatedBy { get; set; }       
    }
}

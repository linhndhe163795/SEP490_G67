namespace MyAPI.DTOs.PaymentRentVehicle
{
    public class TotalPaymentRentVehicleDTO
    {
        public List<PaymentRentVehicelDTO> PaymentRentVehicelDTOs { get; set; } = new List<PaymentRentVehicelDTO>();
        public decimal? Total { get; set; }

    }
}

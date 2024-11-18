using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentRentVehicelController : ControllerBase
    {
        private readonly IPaymentRentVehicleRepository _paymentRentVehicleRepository;
        public PaymentRentVehicelController(IPaymentRentVehicleRepository paymentRentVehicleRepository)
        {
            _paymentRentVehicleRepository = paymentRentVehicleRepository;
        }
        [HttpGet("getPaymentRentVehicle/{timeStart}/{timeEnd}")]
        public async Task<IActionResult> getPaymentRentVehicle(DateTime timeStart, DateTime timeEnd, int? vehicelId, int? vehicelOwner)
        {
            try
            {
                var respone = await _paymentRentVehicleRepository.getPaymentRentVehicleByDate(timeStart, timeEnd, vehicelOwner, vehicelId);
                return Ok(respone);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
    }
}

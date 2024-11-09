using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        public PaymentController(IPaymentRepository  paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CheckHistoryPayment(int amount, string description)
        {
            try
            {

                var isChecked = await _paymentRepository.checkHistoryPayment(amount, description,null,0,0,null);
                if (isChecked)
                {
                    return Ok(new { Message = "Payment successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Payment fail!!! ." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Payment history failed", Details = ex.Message });
            }
        }
    }
}

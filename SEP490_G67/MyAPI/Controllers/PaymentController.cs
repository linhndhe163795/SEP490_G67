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
        public async Task<IActionResult> CheckHistoryPayment(int amout, string description, string codePayment, int ticketID, int typePayment, string email)
        {
            try
            {

                var isChecked = await _paymentRepository.checkHistoryPayment(amout, description, codePayment, ticketID, typePayment, email);
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


        [HttpGet("RandomCode")]
        public async Task<IActionResult> getRandomAsync()
        {
            try
            {
                var randomCode = _paymentRepository.GenerateRandomNumbers();
                if (randomCode != null)
                {
                    return Ok(new { Message = "Payment generate code successfully.", RandomNumbers = randomCode });
                }
                else
                {
                    return BadRequest(new { Message = "Payment generate code fail!!!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Payment history failed", Details = ex.Message });
            }
        }

    }
}

﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentRentVehicelController : ControllerBase
    {
        private readonly IPaymentRentVehicleRepository _paymentRentVehicleRepository;
        private readonly GetInforFromToken _getInforFromToken;
        public PaymentRentVehicelController(IPaymentRentVehicleRepository paymentRentVehicleRepository, GetInforFromToken getInforFromToken)
        {
            _paymentRentVehicleRepository = paymentRentVehicleRepository;
            _getInforFromToken = getInforFromToken;
        }
        [HttpGet("getPaymentRentVehicle")]
        public async Task<IActionResult> getPaymentRentVehicle()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var respone = await _paymentRentVehicleRepository.getPaymentRentVehicleByDate(userId);
                return Ok(respone);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
    }
}

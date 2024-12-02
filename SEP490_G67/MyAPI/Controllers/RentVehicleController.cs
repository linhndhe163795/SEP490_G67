using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using System.Globalization;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentVehicleController : ControllerBase
    {
        private readonly IHistoryRentVehicleRepository _historyRentVehicleRepository;

        public RentVehicleController(IHistoryRentVehicleRepository historyRentVehicleRepository)
        {
            _historyRentVehicleRepository = historyRentVehicleRepository;
        }

        [HttpPost("SendMailRentVehicle")]
        public async Task<IActionResult> SendMailVehicle()
        {
            string desss = "";
            try
            {
                var isSendMail = await _historyRentVehicleRepository.sendMailRequestRentVehicle(desss);
                if (isSendMail)
                {
                    return Ok(new { Message = "Sent mail successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Sent mail denied." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "SendMailVehicle failed", Details = ex.Message });
            }
        }
    }
}

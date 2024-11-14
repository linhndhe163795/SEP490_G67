using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryRentVehicleController : ControllerBase
    {
        private readonly IHistoryRentVehicleRepository _historyRentVehicleRepository;

        public HistoryRentVehicleController(IHistoryRentVehicleRepository historyRentVehicleRepository)
        {
            _historyRentVehicleRepository = historyRentVehicleRepository;
        }

        [HttpGet("ListVehicleRent")]
        public async Task<IActionResult> GetVehicleUseRent()
        {
            try
            {
                var requests = await _historyRentVehicleRepository.historyRentVehicleListDTOs();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle List Rent not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Vehicle List failed", Details = ex.Message });
            }

        }

        [HttpPost("AddHistoryVehicle")]
        public async Task<IActionResult> AddHistoryVehicleUseRent(int requestId, bool choose)
        {
            try
            {
                var requests = await _historyRentVehicleRepository.AccpetOrDeninedRentVehicle(requestId, choose);
                if (requests)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle Rent can't addd");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "History Add Vehicle failed", Details = ex.Message });
            }

        }
    }
}

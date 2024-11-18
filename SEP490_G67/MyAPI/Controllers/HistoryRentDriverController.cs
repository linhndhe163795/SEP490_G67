using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryRentDriverController : ControllerBase
    {
        private readonly IHistoryRentDriverRepository _historyRentDriverRepository;

        public HistoryRentDriverController(IHistoryRentDriverRepository historyRentDriverRepository)
        {
            _historyRentDriverRepository = historyRentDriverRepository;
        }

        [HttpGet("ListDriverRent")]
        public async Task<IActionResult> GetDriverUseRent()
        {
            try
            {
                var requests = await _historyRentDriverRepository.GetListHistoryRentDriver();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Driver List Rent not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Driver Rent failed", Details = ex.Message });
            }
        }

        [HttpPost("AddHistoryDriver")]
        public async Task<IActionResult> AddHistoryDriverUseRent(int requestId, bool choose)
        {
            try
            {
                var requests = await _historyRentDriverRepository.AcceptOrDenyRentDriver(requestId, choose);
                if (requests)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Driver Rent can't be added");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "History Add Driver failed", Details = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("rent-details-with-total-for-owner")]
        public async Task<IActionResult> GetRentDetailsWithTotalForOwner([FromQuery] DateTime startDate,[FromQuery] DateTime endDate)
        {
            try
            {
                // Gọi phương thức repository để lấy thông tin chi tiết các lần thuê và tổng chi phí
                var result = await _historyRentDriverRepository.GetRentDetailsWithTotalForOwner(startDate, endDate);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch rent details for the owner.", error = ex.Message });
            }
        }



    }
}

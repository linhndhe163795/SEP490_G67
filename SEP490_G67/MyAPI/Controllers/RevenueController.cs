using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly RevenueRepository _revenueRepository;
        public RevenueController(RevenueRepository revenueRepository)
        {
            _revenueRepository = revenueRepository;
        }
        [HttpGet("getRevenue/{timeStart}/{timeEnd}")]
        public async Task<IActionResult> getRevenue(DateTime timeStart, DateTime timeEnd, int? vehicleId, int? vehicleOwner)
        {
            try
            {
                var result = await _revenueRepository.RevenueStatistic(timeStart, timeEnd, vehicleId, vehicleOwner);
                return Ok(result);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
       
    }
}

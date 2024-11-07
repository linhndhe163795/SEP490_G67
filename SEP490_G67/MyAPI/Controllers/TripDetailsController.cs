using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripDetailsController : ControllerBase
    {
        private readonly ITripDetailsRepository _tripDetailsRepository;
        public TripDetailsController(ITripDetailsRepository tripDetailsRepository)
        {
            _tripDetailsRepository = tripDetailsRepository;
        }
        
        [HttpGet("tripId")]
        public async Task<IActionResult> getListTripDetailsbyTripId(int TripId)
        {
            try
            {
                var listTripDetail = await _tripDetailsRepository.TripDetailsByTripId(TripId);
                if (listTripDetail == null) return NotFound();
                return Ok(listTripDetail);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTripDetailsbyTripId API: " + ex.Message);
            }
        }
        
        [HttpGet("startPoint/tripId")]
        public async Task<IActionResult> getListstartPointTripDetailsbyTripId(int TripId)
        {
            try
            {
                var listTripDetail = await _tripDetailsRepository.StartPointTripDetailsById(TripId);
                if (listTripDetail == null) return NotFound();
                return Ok(listTripDetail);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTripDetailsbyTripId API: " + ex.Message);
            }
        }
       
        [HttpGet("endPoint/tripId")]
        public async Task<IActionResult> getListendPointTripDetailsbyTripId(int TripId)
        {
            try
            {
                var listTripDetail = await _tripDetailsRepository.EndPointTripDetailsById(TripId);
                if (listTripDetail == null) return NotFound();
                return Ok(listTripDetail);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTripDetailsbyTripId API: " + ex.Message);
            }
        }
    }
}

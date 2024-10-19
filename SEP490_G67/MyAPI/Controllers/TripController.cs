using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;
        public TripController(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetListTrip()
        {
            try
            {
                var listTrip = await _tripRepository.GetListTrip();
                if (listTrip == null)
                {
                    return NotFound("Not found any trip");
                }
                else
                {
                    return Ok(listTrip);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetListTrip: " + ex.Message);
            }

        }
        [HttpGet("searchTrip/startPoint/endPoint/time")]
        public async Task<IActionResult> searchTrip(string startPoint, string endPoint, DateTime time)
        {

            try
            {
                var timeonly = time.ToString("HH:ss:mm");

                var searchTrip = await _tripRepository.SreachTrip(startPoint, endPoint, timeonly);
                if (searchTrip == null) return NotFound("Not found trip");
                return Ok(searchTrip);
            }
            catch (Exception ex)
            {
                return BadRequest("searchTripAPI: " + ex.Message);
            }
        }
        [HttpPost("addTrip")]
        public async Task<IActionResult> addTrip(TripDTO trip)
        {
            try
            {
                _tripRepository.AddTrip(trip);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

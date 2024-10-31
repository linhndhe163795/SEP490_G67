using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TripController(ITripRepository tripRepository,  IHttpContextAccessor httpContextAccessor, GetInforFromToken getInforFromToken)
        {
            _tripRepository = tripRepository;
            _httpContextAccessor = httpContextAccessor;
            _getInforFromToken = getInforFromToken;
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
        public async Task<IActionResult> searchTrip(string startPoint,  string endPoint, DateTime time)
        {

            try
            {
                var timeonly = time.ToString("HH:ss:mm");
                var date = time.ToString();
                var searchTrip = await _tripRepository.SreachTrip(startPoint, endPoint, timeonly);
                _httpContextAccessor.HttpContext.Session.SetString("date", date);
                if (searchTrip == null) return NotFound("Not found trip");
                return Ok(searchTrip);
            }
            catch (Exception ex)
            {
                return BadRequest("searchTripAPI: " + ex.Message);
            }
        }
        [HttpPost("addTrip")]
        public async Task<IActionResult> addTrip(TripDTO trip, int vehicleId)
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
                await _tripRepository.AddTrip(trip, vehicleId, userId);

                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("updateTrip/{id}")]
        public async Task<IActionResult> updateTrip(int id, TripDTO tripDTO)
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

                await _tripRepository.UpdateTripById(id, tripDTO, userId);
                return Ok(tripDTO);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("updateStatusTrip/{id}")]
        public async Task<IActionResult> updateStatusTrip(int id)
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
                await _tripRepository.updateStatusTrip(id, userId);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

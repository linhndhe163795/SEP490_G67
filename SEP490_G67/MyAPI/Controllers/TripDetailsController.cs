using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripDetailsController : ControllerBase
    {
        private readonly GetInforFromToken _getInforFromToken;
        private readonly ITripDetailsRepository _tripDetailsRepository;
        public TripDetailsController(ITripDetailsRepository tripDetailsRepository, GetInforFromToken getInforFromToken)
        {
            _tripDetailsRepository = tripDetailsRepository;
            _getInforFromToken = getInforFromToken;
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
        [HttpPost("updateTripDetails/{tripId}/{tripDetailsId}")]
        public async Task<IActionResult> updateTripDetailsById(int tripId, int tripDetailsId, UpdateTripDetails updateTripDetails)
        {
            try
            {
                await _tripDetailsRepository.UpdateTripDetailsById(tripId, tripDetailsId, updateTripDetails);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("addTripDetails/{tripId}")]
        public async Task<IActionResult> addTripDetailsByTripId(int tripId, TripDetailsDTO tripDetailsDTO)
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
                await _tripDetailsRepository.AddTripDetailsByTripId(tripId, tripDetailsDTO,userId);
                return Ok("Add tripdetails success");
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetTripDetailData/{tripDetailId}")]
        public async Task<IActionResult> GetTripDetailById(int tripDetailId)
        {
            try
            {
                var tripDetail = await _tripDetailsRepository.GetTripDetailById(tripDetailId);
                return Ok(tripDetail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }   
}

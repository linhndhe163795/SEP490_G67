using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleTripController : ControllerBase
    {
        private readonly IVehicleTripRepository _vehicleTripRepository;
        private readonly GetInforFromToken _inforFromToken;
        public VehicleTripController(IVehicleTripRepository vehicleTripRepository, GetInforFromToken inforFromToken)
        {
            _vehicleTripRepository = vehicleTripRepository;
            _inforFromToken = inforFromToken;
        }
        [HttpPost("assginVehicleToTrip")]
        public async Task<IActionResult> assginVehicleToTrip( List<int> vehicleId, int tripId)
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
                var staffId = _inforFromToken.GetIdInHeader(token);
                await _vehicleTripRepository.assginVehicleToTrip(staffId, vehicleId, tripId);
                return Ok(new
                {
                    vehicleId,
                    tripId,
                    staffId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[HttpPut("updateVehicleTrip")]
        //public async Task<IActionResult> updateVehicleTrip(int tripId, List<>)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}

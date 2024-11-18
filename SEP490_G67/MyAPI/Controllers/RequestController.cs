using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.DTOs.HistoryRentVehicle;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using System.Data;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IUserCancleTicketRepository _userCancleTicketRepository;
        private readonly GetInforFromToken _token;

        public RequestController(IRequestRepository requestRepository, GetInforFromToken token)
        {
            _token = token;
            _requestRepository = requestRepository;
        }
        [Authorize(Roles = "Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _requestRepository.GetAllRequestsWithDetailsAsync();
            return Ok(requests);
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var request = await _requestRepository.GetRequestWithDetailsByIdAsync(id);
            if (request == null)
                return NotFound();
            return Ok(request);
        }
        
        [HttpPost("/CreateTicketForRentCar")]
        public async Task<IActionResult> CreateRequestForRentCar(RequestDTO requestWithDetailsDto)
        {
            var createdRequest = await _requestRepository.CreateRequestRentCarAsync(requestWithDetailsDto);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.Id }, createdRequest);
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("/UpdateRequestForRentCar/{id}")]
        public async Task<IActionResult> UpdateRequestForRentCar(int id, RequestDTO requestDto)
        {
            var updated = await _requestRepository.UpdateRequestRentCarAsync(id, requestDto);
            if (updated == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPost("/CreateTicketForHireDriver")]
        public async Task<IActionResult> CreateRequestForHireDriver(RequestDTO requestWithDetailsDto)
        {
            var createdRequest = await _requestRepository.CreateRequestRentCarAsync(requestWithDetailsDto);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.Id }, createdRequest);
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("/UpdateRequestForHireDriver/{id}")]
        public async Task<IActionResult> UpdateRequestForHireDriver(int id, RequestDTO requestDto)
        {
            var updated = await _requestRepository.UpdateRequestRentCarAsync(id, requestDto);
            if (updated == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        [Authorize(Roles = "Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {

            var request = await _requestRepository.GetRequestWithDetailsByIdAsync(id);
            if (request == null)
                return NotFound();


            foreach (var detail in request.RequestDetails)
            {
                await _requestRepository.DeleteRequestDetailAsync(request.Id, detail.DetailId);
            }


            await _requestRepository.Delete(request);

            return NoContent();
        }

        [HttpPost("accept/{id}")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            try
            {
                await _requestRepository.AcceptRequestAsync(id);
                return Ok("Request accepted.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deny/{id}")]
        public async Task<IActionResult> DenyRequest(int id)
        {
            try
            {
                await _requestRepository.DenyRequestAsync(id);
                return Ok("Request denied.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("acceptCancleTicket/{id}")]
        public async Task<IActionResult> AcceptCancleTicketRequest(int id)
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
                var staffId = _token.GetIdInHeader(token);
                await _requestRepository.updateStatusRequestCancleTicket(id, staffId);
                return Ok("update success");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("listRequestCancleTicket")]
        public async Task<IActionResult> listRequestCancleTicket()
        {
            try
            {
                var listRequestCancle = await _requestRepository.getListRequestCancle();
                return Ok(listRequestCancle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // create request from user
        [Authorize]
        [HttpPost("createRequestCancleTicket")]
        public async Task<IActionResult> createRequestCanleTicket(RequestCancleTicketDTOs requestCancleTicketDTOs)
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
                var userId = _token.GetIdInHeader(token);
                await _requestRepository.createRequestCancleTicket(requestCancleTicketDTOs, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("addRentVehicleRequest")]
        public async Task<IActionResult> AddVehicle(RentVehicleAddDTO rentVehicleAddDTO)
        {
            try
            {

                var isAdded = await _requestRepository.CreateRequestRentVehicleAsync(rentVehicleAddDTO);
                return Ok(new { Message = "Vehicle rent added successfully.", VehicleRent = rentVehicleAddDTO });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle rent Add failed", Details = ex.Message });
            }
        }

        [HttpPost("addRenDriverRequest")]
        public async Task<IActionResult> RentDriver(RequestDetailForRentDriver rentVehicleAddDTO)
        {
            try
            {

                var isAdded = await _requestRepository.CreateRequestRentDriverAsync(rentVehicleAddDTO);
                return Ok(new { Message = "Vehicle rent added successfully.", VehicleRent = rentVehicleAddDTO });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle rent Add failed", Details = ex.Message });
            }
        }

        [HttpPost("ConvenientTripCreateForUser")]
        public async Task<IActionResult> CreateRequestConvenientTrip(ConvenientTripDTO convenientTripDTO)
        {
            try
            {
                var result = await _requestRepository.CreateRequestCovenient(convenientTripDTO);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Convenient trip request created successfully."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "Failed to create convenient trip request."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("ConvenientTripUpdateForStaff")]
        public async Task<IActionResult> UpdateRequestConvenientTrip(int requestId, bool choose)
        {
            try
            {
                var result = await _requestRepository.UpdateStatusRequestConvenient(requestId, choose);
                return Ok(new { success = true, message = "Request updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}

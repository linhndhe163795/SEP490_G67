using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.DriverDTOs;
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
        private readonly IMapper _mapper;
        private readonly Jwt _Jwt;
        private readonly GetInforFromToken _getInforFromToken;

        public RequestController(IRequestRepository requestRepository, GetInforFromToken token, IMapper mapper, Jwt jwt, GetInforFromToken getInforFromToken)
        {
            _token = token;
            _requestRepository = requestRepository;
            _mapper = mapper;
            _Jwt = jwt;
            _getInforFromToken = getInforFromToken;
        }
        [Authorize(Roles = "Staff, VehicleOwner, Driver,Admin")]
        [HttpGet]
        public async Task<IActionResult> getListRequest()
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
                var role = _getInforFromToken.GetRoleFromToken(token);
                var listHistoryRentDriver = await _requestRepository.GetRequestsByRole(userId, role);
                return Ok(listHistoryRentDriver);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var requests = await _requestRepository.Get(id);
            if (requests == null)
            {
                return NotFound("Driver not found");
            }

            var UpdateRequestDto = _mapper.Map<RequestDTO>(requests);
            return Ok(UpdateRequestDto);
        }
        [HttpPost("/CreateTicketForRentFullCar")]
        public async Task<IActionResult> CreateRequestForRentCar(RequestDTOForRentCar requestforrentcar)
        {
            try
            {
                var isAdded = await _requestRepository.CreateRequestRentCarAsync(requestforrentcar);
                return Ok(new { Message = "Car rent added successfully.", CarRent = requestforrentcar });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddDriver rent Add failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("/UpdateRequestForRentFullCar/{id}")]
        public async Task<IActionResult> UpdateRequestForRentCar(int id, RequestDTOForRentCar requestDto)
        {
            var isupdated = await _requestRepository.UpdateRequestRentCarAsync(id, requestDto);
            if (isupdated == false)
            {
                return BadRequest();
            }
            return Ok("Update success!");
        }

        [Authorize(Roles = "Staff,Driver,VehicleOwner")]
        [HttpGet("/GetRequestDetailById/{id}")]
        public async Task<IActionResult> GetRequestDetail(int id)
        {
            var requestdetail = await _requestRepository.GetRequestDetailByIdAsync(id);
            if (requestdetail == null)
            {
                return NotFound();
            }
            return Ok(requestdetail);
        }
        [Authorize(Roles = "Staff,Driver,VehicleOwner")]
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            try
            {
                var request = await _requestRepository.GetRequestByIdAsync(id);
                if (request == null)
                {
                    return NotFound("Request not found.");
                }

                if (request.Note != "Đã xác nhận")
                {
                    return BadRequest("Only requests with 'Đã xác nhận' note can be deleted.");
                }

                await _requestRepository.DeleteRequestWithDetailsAsync(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error while deleting request: {ex.Message}");
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("acceptCancleTicket/{id}")]
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
        [Authorize(Roles = "Driver")]
        [HttpPost("CreateRentVehicleForDriverRequest")]
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
        [Authorize(Roles = "VehicleOwner")]
        [HttpPost("CreateRentDriverForOwnerRequest")]
        public async Task<IActionResult> RentDriver(RequestDetailForRentDriver rentDriverAddDTO)
        {
            try
            {

                var isAdded = await _requestRepository.CreateRequestRentDriverAsync(rentDriverAddDTO);
                return Ok(new { Message = "Driver rent added successfully.", DriverRent = rentDriverAddDTO });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddDriver rent Add failed", Details = ex.Message });
            }
        }
        [Authorize]
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
        [Authorize(Roles = "Staff")]
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
        //screen request for user
        [Authorize]
        [HttpGet("getListRequestForUser")]
        public async Task<IActionResult> getListRequestForVehicleOwner()
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
                var result = await _requestRepository.getListRequestForUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //update request for user
        [Authorize]
        [HttpPost("updateRequestForUser")]
        public async Task<IActionResult> updateRequestDetails([FromBody] RequestDetailDTO requestDetailDTO)
        {
            try
            {
                await _requestRepository.updateRequest(requestDetailDTO);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("getListRequestForDriver")]
        public async Task<IActionResult> getListRequestForDriver()
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
                var driverid = _token.GetIdInHeader(token);
                var result = await _requestRepository.GetListRequestForDriver(driverid);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
    }
}

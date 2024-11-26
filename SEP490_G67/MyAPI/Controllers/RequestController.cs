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

        public RequestController(IRequestRepository requestRepository, GetInforFromToken token, IMapper mapper, Jwt jwt)
        {
            _token = token;
            _requestRepository = requestRepository;
            _mapper = mapper;
            _Jwt = jwt;
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllRequest()
        {
            var requests = await _requestRepository.GetAll();
            var UpdateRequestDto = _mapper.Map<IEnumerable<RequestDTO>>(requests);
            return Ok(UpdateRequestDto);
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
                return Ok(new { Message = "Car rent added successfully.",CarRent = requestforrentcar });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddDriver rent Add failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("/UpdateRequestForRentFullCar/{id}")]
        public async Task<IActionResult> UpdateRequestForRentCar(int id, RequestDTOForRentCar requestDto)
        {
            var isupdated = await _requestRepository.UpdateRequestRentCarAsync(id, requestDto);
            if (isupdated == false)
            {
                return BadRequest();
            }
            return Ok("Update success!");
        }

        [Authorize(Roles = "Staff")]
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


        [Authorize(Roles = "Staff")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {

            var requests = await _requestRepository.Get(id);
            if (requests == null)
            {
                return NotFound("request not found");
            } 
            await _requestRepository.Delete(requests);

            return NoContent();
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
        //driver thuê xe trong hệ thống
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

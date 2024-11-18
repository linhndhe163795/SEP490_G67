using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IMapper _mapper;
        public TicketController(ITicketRepository ticketRepository, GetInforFromToken getInforFromToken, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _getInforFromToken = getInforFromToken;
        }
        [Authorize]
        [HttpPost("bookTicket/{tripDetailsId}")]
        public async Task<IActionResult> createTicket(BookTicketDTOs ticketDTOs, int tripDetailsId, string? promotionCode, int numberTicket)
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

                var ticketId = await _ticketRepository.CreateTicketByUser(promotionCode, tripDetailsId, ticketDTOs, userId, numberTicket);
                return Ok(new { ticketId, ticketDetails = ticketDTOs });
            }
            catch (Exception ex)
            {
                return BadRequest("createTicket: " + ex.Message);
            }
        }

        [Authorize(Roles = "Staff")]
        [HttpPost("createTicketFromDriver/{vehicleId}")]
        public async Task<IActionResult> creatTicketFromDriver([FromBody] TicketFromDriverDTOs ticketFromDriver, [FromForm] int vehicleId, int numberTicket)
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
                var driverId = _getInforFromToken.GetIdInHeader(token);

                var priceTrip = await _ticketRepository.GetPriceFromPoint(ticketFromDriver, vehicleId);
                await _ticketRepository.CreatTicketFromDriver(priceTrip, vehicleId, ticketFromDriver, driverId, numberTicket);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("createTicketForRentCar/{vehicleId}")]
        public async Task<IActionResult> CreateTicketForRentCar([FromBody] TicketForRentCarDTO ticketRentalDTO, int vehicleId, decimal price, int userId)
        {
            if (ticketRentalDTO == null)
            {
                return BadRequest("Invalid ticket rental data.");
            }

            try
            {
                await _ticketRepository.CreateTicketForRentCar(vehicleId, price, ticketRentalDTO, userId);

                return Ok("Ticket created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> getListTicket()
        {
            try
            {
                var listTicket = await _ticketRepository.getAllTicket();
                if (listTicket == null) return NotFound();
                var listTickerMapper = _mapper.Map<List<ListTicketDTOs>>(listTicket);
                return Ok(listTickerMapper);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTicket: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("tickeNotPaid")]
        public async Task<IActionResult> getListTicketNotPaid(int vehicleId)
        {
            try
            {
                var listTicket = await _ticketRepository.GetListTicketNotPaid(vehicleId);
                if (listTicket == null) return NotFound();
                return Ok(listTicket);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTicket: " + ex.Message);
            }
        }
        [Authorize]
        [HttpGet("ticketById/{ticketId}")]
        public async Task<IActionResult> getTicketByTicketId(int ticketId)
        {
            try
            {
                var ticketById = await _ticketRepository.getTicketById(ticketId);
                return Ok(ticketById);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("updateStatusticketNotPaid/id")]
        public async Task<IActionResult> updateStatusTicketNotPaid(int id)
        {
            try
            {
                await _ticketRepository.UpdateStatusTicketNotPaid(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "VehicleOwner, Staff")]
        [HttpGet("RevenueTicket")]
        public async Task<IActionResult> getRevenueTicket(int? vehicle, int? vehicleOwner)
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
                var respone = await _ticketRepository.getRevenueTicket(vehicle, vehicleOwner, userId);
                return Ok(respone);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

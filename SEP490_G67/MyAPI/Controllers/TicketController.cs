using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

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
        [HttpPost("bookTicket")]
        public async Task<IActionResult> createTicket(BookTicketDTOs ticketDTOs, int tripDetailsId, string? promotionCode)
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

                await _ticketRepository.CreateTicketByUser(promotionCode, tripDetailsId, ticketDTOs, userId);
                return Ok(ticketDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest("createTicket: " + ex.Message);
            }
        }
        [HttpPost("bookTicketBeforetUsePromotion")]
        public async Task<IActionResult> createTicketBefortUsePromotion(BookTicketDTOs ticketDTOs, int tripDetailsId, string? promotionCode)
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

                await _ticketRepository.CreateTicketByUser(promotionCode, tripDetailsId, ticketDTOs, userId);
                return Ok(ticketDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest("createTicket: " + ex.Message);
            }
        }
        [HttpPost("createTicketFromDriver/{vehicleId}")]
        public async Task<IActionResult> creatTicketFromDriver([FromBody] TicketFromDriverDTOs ticketFromDriver, [FromForm] int vehicleId)
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
                await _ticketRepository.CreatTicketFromDriver(priceTrip, vehicleId, ticketFromDriver, driverId);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
    }
}

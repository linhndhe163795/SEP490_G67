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
        [HttpPost]
        public async Task<IActionResult> createTicket( TicketDTOs ticketDTOs,  int tripDetailsId, [FromQuery] string? promotionCode ,  int uid)
        {
            try
            {
                //string token = Request.Headers["Authorization"];
                //if (token.StartsWith("Bearer"))
                //{
                //    token = token.Substring("Bearer ".Length).Trim();
                //}
                //if (string.IsNullOrEmpty(token))
                //{
                //    return BadRequest("Token is required.");
                //}
                //var userId = _getInforFromToken.GetIdInHeader(token);

                await _ticketRepository.CreateTicketByUser(promotionCode, tripDetailsId, ticketDTOs, uid);
                return Ok(ticketDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest("createTicket: " + ex.Message);
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
    }
}

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
        public TicketController(ITicketRepository ticketRepository, GetInforFromToken getInforFromToken)
        {
            _ticketRepository = ticketRepository;
            _getInforFromToken = getInforFromToken;
        }
        [HttpPost]
        public async Task<IActionResult> createTicket(TicketDTOs ticketDTOs, int tripDetailsId, int? Promotion ,int uid)
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

                await _ticketRepository.CreateTicketByUser(Promotion, tripDetailsId, ticketDTOs, uid);
                return Ok(ticketDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest("createTicket: " + ex.Message);
            }
        }

    }
}

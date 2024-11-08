using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.UserCancleTicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCancleTicketController : ControllerBase
    {
        private readonly IUserCancleTicketRepository _userCancleTicketRepository;
        private readonly GetInforFromToken _getInforFromToken;
        public UserCancleTicketController(IUserCancleTicketRepository userCancleTicketRepository, GetInforFromToken getInforFromToken)
        {
            _userCancleTicketRepository = userCancleTicketRepository;
            _getInforFromToken = getInforFromToken;
        }

        [HttpPost("userCancleTicket")]
        public async Task<IActionResult> addUserCancleTicket(AddUserCancleTicketDTOs addUserCancleTicketDTOs)
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
                await _userCancleTicketRepository.AddUserCancleTicket(addUserCancleTicketDTOs, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

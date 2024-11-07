using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointUserController : ControllerBase
    {
        private readonly IPointUserRepository _pointUserRepository;
        private readonly GetInforFromToken _getInforFromToken;
        public PointUserController(IPointUserRepository pointUserRepository, GetInforFromToken getInforFromToken) 
        {
            _pointUserRepository = pointUserRepository;
            _getInforFromToken = getInforFromToken;
        }
        [Authorize]
        [HttpGet("getPointUserByUserId")]
        public async Task<IActionResult> getPointUserByUserId()
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
                var pointUser = await _pointUserRepository.getPointUserById(userId);
                return Ok(pointUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            } 
        }
       
    }
}

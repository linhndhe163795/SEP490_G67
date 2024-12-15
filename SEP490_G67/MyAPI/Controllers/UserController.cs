using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using System.IO;


namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly GetInforFromToken _getInforFromToken;
        

        public UserController(IUserRepository userRepository, SendMail sendMailHelper, GetInforFromToken getInforFromToken)
        {
            _userRepository = userRepository;
            _getInforFromToken = getInforFromToken;
            
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
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
                await _userRepository.ChangePassword(changePasswordDTO, userId);

                
                return Ok("Change password successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }

        [Authorize]
        [HttpPost("EditProfile")]
        public async Task<IActionResult> EditProfile([FromForm] EditProfileDTO editProfileDTO)
        {
            try
            {
               
                var updatedUser = await _userRepository.EditProfile(editProfileDTO);
                return Ok("Update user profile successfull");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error:  " + ex.Message);
            }
        }
        [Authorize]
        [HttpGet("listVehicleOwner")]
        public async Task<IActionResult> listVehicleOwner()
        {
            try
            {
                var listVehicle = await _userRepository.getListVehicleOwner();
                return Ok(listVehicle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

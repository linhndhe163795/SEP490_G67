using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        

        public UserController(IUserRepository userRepository, SendMail sendMailHelper)
        {
            _userRepository = userRepository;
            
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                await _userRepository.ChangePassword(changePasswordDTO);

                
                return Ok("Change password successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }

        [Authorize]
        [HttpPut("EditProfile/{userId}")]
        public async Task<IActionResult> EditProfile(int userId, EditProfileDTO editProfileDTO)
        {
            try
            {
                var updatedUser = await _userRepository.EditProfile( editProfileDTO);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error:  " + ex.Message);
            }
        }

    }
}

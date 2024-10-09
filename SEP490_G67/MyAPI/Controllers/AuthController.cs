using AutoMapper;
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
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly SendMail _sendMail;
        private readonly IMapper _mapper;
        private readonly Jwt _Jwt;
        public AuthController(IUserRepository userRepository, Jwt Jwt, IUserRoleRepository userRoleRepository, SendMail sendMail, IMapper mapper)
        {
            _mapper = mapper;
            _sendMail = sendMail;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _Jwt = Jwt;
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterDTO user)
        {
            try
            {
                var userMapper = _mapper.Map<User>(user);
                bool checkAccount = await _userRepository.checkAccountExsit(userMapper);
                if (!checkAccount)
                {
                    await _userRepository.Register(user);
                    var lastIdUser = _userRepository.lastIdUser();
                    UserRole ur = new UserRole
                    {
                        RoleId = 3,
                        Status = true,
                        UserId = lastIdUser.Result
                    };
                    await _userRoleRepository.Add(ur);
                    return Ok(user);
                }
                return BadRequest("ton tai account");
            }
            catch (Exception ex)
            {
                throw new Exception("Register Failed " + ex.Message);
            }

        }
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(ConfirmCode code)
        {
            try
            {
                var user = await _userRepository.confirmCode(code);
                if (user)
                {
                    return Ok();
                }
                return Ok("Incorrect code");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed Confirm: " + ex.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDTO userLogin)
        {
            try
            {
                if (await _userRepository.checkLogin(userLogin))
                {
                    var getUserLogin = await _userRepository.GetUserLogin(userLogin);
                    var tokenString = _Jwt.CreateToken(getUserLogin);
                    return Ok(tokenString);
                }
                else
                {
                    return NotFound("Incorrect Email or Password");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Login Failed" + ex.Message);
            }
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPassword)
        {
            try
            {
                var userMapper = _mapper.Map<User>(forgotPassword);
                if (await _userRepository.checkAccountExsit(userMapper))
                {
                    await _userRepository.ForgotPassword(forgotPassword);
                    return Ok();
                }
                else
                {
                    return NotFound("Not found account");
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Failed: " + ex.Message);
            }
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                await _userRepository.ResetPassword(resetPasswordDTO);
                return Ok("Change Password successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw new Exception("Failed " + ex.Message);
            }
        }

    }
}

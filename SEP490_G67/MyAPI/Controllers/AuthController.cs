using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs;
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
        public AuthController(IUserRepository userRepository, IUserRoleRepository userRoleRepository,SendMail sendMail ,IMapper mapper) 
        {
            _mapper = mapper;
            _sendMail = sendMail;
            _userRoleRepository = userRoleRepository;
            _userRepository =  userRepository;  
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterDTO user)
        {
            bool checkAccount = _userRepository.checkAccountExsit(user);
            var userMapper = _mapper.Map<User>(user);
            if (!checkAccount)
            {
                var verifyCode = _sendMail.GenerateVerificationCode(4);
                SendMailDTO sendMailDTO = new()
                {
                    FromEmail = "duclinh5122002@gmail.com",
                    Password = "jetj haze ijdw euci",
                    ToEmail = user.Email,
                    Subject = "Verify Code",
                    Body = verifyCode,
                };
                if (await _sendMail.SendEmail(sendMailDTO)) 
                {
                    await _userRepository.Register(user);
                }
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
    }
}

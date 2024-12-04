using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleOwnerDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly GetInforFromToken _getInforFromToken;
        public AccountController(IAccountRepository accountRepository, IMapper mapper, IUserRepository userRepository, GetInforFromToken getInforFromToken)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _getInforFromToken = getInforFromToken;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("listAccount")]
        public async Task<IActionResult> GetListAccount()
        {
            try
            {
                var listAccount = await _accountRepository.GetListAccount();
                if (listAccount != null)
                {
                    return Ok(listAccount);
                }
                else
                {
                    return NotFound("Not found list Account");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Account get list failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("detailsAccount/{id}")]
        public async Task<IActionResult> GetDetailsAccountById(int id)
        {
            try
            {
                var account = await _accountRepository.GetDetailsUser(id);
                if (account != null)
                {
                    return Ok(account);
                }
                else
                {
                    return NotFound("Account not exits");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Account details failed", Details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("deleteAccount/{id}")]
        public async Task<IActionResult> DelteAccountById(int id)
        {
            try
            {
                var account = await _accountRepository.DeteleAccount(id);
                if (account == true)
                {
                    return Ok("Delte Success!!");
                }
                else
                {
                    return NotFound("Account not exits");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Account delete failed", Details = ex.Message });
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("updateAccount/{id}/{newIdUpdate}")]
        public async Task<IActionResult> UpdateAccountById(int id, int newIdUpdate)
        {
            try
            {
                var accountUpdated = await _accountRepository.UpdateRoleOfAccount(id, newIdUpdate);

                return Ok(new { Message = "Account Update successfully." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Account update failed", Details = ex.Message });
            }

        }
        [Authorize(Roles = "Admin")]
        [HttpGet("listRole")]
        public async Task<IActionResult> GetListRole()
        {
            try
            {
                var listRole = await _accountRepository.GetListRole();
                if (listRole != null)
                {
                    return Ok(listRole);
                }
                else
                {
                    return NotFound("Not found list Role");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Account list failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("registerVehicleOwner")]
        public async Task<IActionResult> RegisterVehicleOwner(VehicleOwnerDTO vehicleOwnerDTO)
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
                var staffId = _getInforFromToken.GetIdInHeader(token);
                await _userRepository.RegisterVehicleOwner(vehicleOwnerDTO, staffId);
                return Ok(vehicleOwnerDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

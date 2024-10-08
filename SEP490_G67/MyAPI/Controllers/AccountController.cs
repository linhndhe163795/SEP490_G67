using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.UserDTOs;
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
        public AccountController(IAccountRepository accountRepository,  IMapper mapper)
        {
            _accountRepository = accountRepository;
        }

        [HttpPost("listAccount")]
        public async Task<IActionResult> GetListAccount()
        {
            try
            {
                var listAccount = await _accountRepository.GetListAccount();
                if(listAccount != null)
                {
                    return Ok(listAccount);
                }else
                {
                    return NotFound("Not found list Account");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetListAccount Failed " + ex.Message);
            }

        }

        [HttpPost("detailsAccount/{id}")]
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
                throw new Exception("GetDetailsAccount Failed " + ex.Message);
            }

        }


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
                throw new Exception("DeleteAccount Failed " + ex.Message);
            }

        }

        

        [HttpPost("updateAccount/{id}/{roleId}/{userIdUpdate}")]
        public async Task<IActionResult> UpdateAccountById(int id, int roleId, int userIdUpdate)
        {
            try
            {
                var accountUpdated = await _accountRepository.UpdateRoleOfAccount(id, roleId, userIdUpdate);

                if (accountUpdated)
                {
                    return Ok("Update Success!!");
                }
                else
                {
                    return NotFound("Account does not exist");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("DeleteAccount Failed " + ex.Message);
            }
        }

        [HttpPost("listRole")]
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
                throw new Exception("GetListRole Failed " + ex.Message);
            }

        }







    }
}

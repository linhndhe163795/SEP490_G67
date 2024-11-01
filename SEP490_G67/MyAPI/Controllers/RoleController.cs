using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.RoleDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }


        [HttpGet("getListRole")]
        public async Task<IActionResult> GetAllRole()
        {
            try
            {
                var listRole = await _roleRepository.GetListRole();

                if(listRole != null)
                {
                    return Ok(listRole);
                }else
                {
                    return NotFound("Not found list Role");
                }
            } catch (Exception ex)
            {
                return BadRequest(new { Message = "Role get list failed", Details = ex.Message });
            }

        }

        [HttpPost("addRole")]
        public async Task<IActionResult> AddRole(RoleAddDTO roleAddDTO)
        {

            try
            {
                var addRole = await _roleRepository.AddRole(roleAddDTO);
                return Ok(new { Message = "Role added successfully.", Role = roleAddDTO });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Add Role failed", Details = ex.Message });
            }

        }


        [HttpPut("updateRole/{id}")]
        public async Task<IActionResult> UpdateRole(int id,RoleAddDTO roleAddDTO)
        {

            try
            {
                var addRole = await _roleRepository.UpdateRole(id,roleAddDTO);
                return Ok(new { Message = "Role update successfully.", Role = roleAddDTO });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Update Role failed", Details = ex.Message });
            }
        }

        [HttpDelete("deleteRole/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var addRole = await _roleRepository.DeleteRole(id);
                return Ok(new { Message = "Role delete successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Delete Role failed", Details = ex.Message });
            }
        }

    }
}

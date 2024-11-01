using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.LossCostDTOs.LossCostTypeDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LossCostTypeController : ControllerBase
    {
        private readonly ILossCostTypeRepository _lossCostTypeRepository;
        public LossCostTypeController(ILossCostTypeRepository lossCostTypeRepository)
        {
            _lossCostTypeRepository = lossCostTypeRepository;
        }


        [HttpGet("listLossCostType")]
        public async Task<IActionResult> GetLossCostType()
        {
            try
            {
                var requests = await _lossCostTypeRepository.GetAllList();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Loss Cost Type null");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Loss Cost Typle failed", Details = ex.Message });
            }

        }


        [HttpPost("addLossCostType")]
        public async Task<IActionResult> AddLossCostType(LossCostTypeAddDTO lossCostTypeAddDTO)
        {
            try
            {

                var isAdded = await _lossCostTypeRepository.CreateLossCostType(lossCostTypeAddDTO);
                return Ok(new { Message = "LossCostType added successfully.", Vehicle = lossCostTypeAddDTO });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "LossCostType add failed", Details = ex.Message });
            }
        }

        [HttpPut("updateLossCostType/{id}")]
        public async Task<IActionResult> UpdateLossCostType(int id, LossCostTypeAddDTO lossCostTypeAddDTO)
        {
            try
            {
                var checkUpdate = await _lossCostTypeRepository.UpdateLossCostType(id, lossCostTypeAddDTO);

                return Ok(new { Message = "LossCostType Update successfully." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "LossCostType update failed", Details = ex.Message });
            }

        }

        [HttpDelete("deleteLossCostType/{id}")]
        public async Task<IActionResult> UpdateLossCostType(int id)
        {
            try
            {
                var delete = await _lossCostTypeRepository.DeleteLossCostType(id);

                if (delete)
                {
                    return Ok(new { Message = "LossCostType delete successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "LossCostType delete failed." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "LossCostType Delete failed", Details = ex.Message });
            }
        }
    }
}

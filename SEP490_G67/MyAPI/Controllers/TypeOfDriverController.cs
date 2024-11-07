using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeOfDriverController : ControllerBase
    {
        private readonly ITypeOfDriverRepository _typeOfDriverRepository;

        public TypeOfDriverController(ITypeOfDriverRepository typeOfDriverRepository)
        {
            _typeOfDriverRepository = typeOfDriverRepository;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTypeOfDriver([FromBody] UpdateTypeOfDriverDTO updateTypeOfDriverDto)
        {
            if (updateTypeOfDriverDto == null)
            {
                return BadRequest("Invalid data");
            }

            var typeOfDriver = await _typeOfDriverRepository.CreateTypeOfDriverAsync(updateTypeOfDriverDto);
            return CreatedAtAction(nameof(GetTypeOfDriverById), new { id = typeOfDriver.Id }, typeOfDriver);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTypeOfDriver(int id, [FromBody] UpdateTypeOfDriverDTO updateTypeOfDriverDto)
        {
            if (updateTypeOfDriverDto == null)
            {
                return BadRequest("Invalid data");
            }

            try
            {
                var updatedTypeOfDriver = await _typeOfDriverRepository.UpdateTypeOfDriverAsync(id, updateTypeOfDriverDto);
                return Ok(updatedTypeOfDriver);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [Authorize(Roles = "Staff,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeOfDriver(int id)
        {
            var typeOfDriver = await _typeOfDriverRepository.Get(id);
            if (typeOfDriver == null)
            {
                return NotFound("Type of driver not found");
            }

            await _typeOfDriverRepository.Delete(typeOfDriver);
            return Ok("Deleted successfully");
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTypeOfDriverById(int id)
        {
            var typeOfDriver = await _typeOfDriverRepository.Get(id);
            if (typeOfDriver == null)
            {
                return NotFound("Type of driver not found");
            }

            return Ok(typeOfDriver);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllTypeOfDrivers()
        {
            var typesOfDrivers = await _typeOfDriverRepository.GetAll();
            return Ok(typesOfDrivers);
        }
    }
}

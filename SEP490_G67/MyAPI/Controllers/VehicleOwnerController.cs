using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.VehicleOwnerDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleOwnerController : ControllerBase
    {
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;

        public VehicleOwnerController(IVehicleOwnerRepository vehicleOwnerRepository)
        {
            _vehicleOwnerRepository = vehicleOwnerRepository;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllVehicleOwners()
        {
            var owners = await _vehicleOwnerRepository.GetAll();
            return Ok(owners);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleOwner(int id)
        {
            var owner = await _vehicleOwnerRepository.Get(id);
            if (owner == null)
            {
                return NotFound();
            }
            return Ok(owner);
        }


        [HttpPost]
        public async Task<IActionResult> AddVehicleOwner([FromBody] VehicleOwnerDTO newOwner)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var owner = await _vehicleOwnerRepository.CreateOwnerAsync(newOwner); // Sử dụng phương thức CreateVehicleOwnerAsync
            return CreatedAtAction(nameof(GetVehicleOwner), new { id = owner.Id }, owner);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicleOwner(int id, [FromBody] VehicleOwnerDTO updatedOwner)
        {
            

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var owner = await _vehicleOwnerRepository.UpdateOwnerAsync(id, updatedOwner);
            return Ok(owner);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleOwner(int id)
        {
            var owner = await _vehicleOwnerRepository.Get(id);
            if (owner == null)
            {
                return NotFound();
            }

            await _vehicleOwnerRepository.Delete(owner);
            return NoContent();
        }
    }
}

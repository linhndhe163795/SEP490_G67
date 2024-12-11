using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.TypeOfDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeOfTripController : ControllerBase
    {
        private readonly ITypeOfTripRepository _typeOfTripRepository;

        public TypeOfTripController(ITypeOfTripRepository typeOfTripRepository)
        {
            _typeOfTripRepository = typeOfTripRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTypeOfTrips()
        {
            var typesOfTrips = await _typeOfTripRepository.GetAll();
            var typeOfTripDtos = typesOfTrips.Select(r => new TypeOfTripDTO
            {
                Id = r.Id,
                Description = r.Description 
            }).ToList();

            return Ok(typeOfTripDtos);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.TypeOfDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeOfRequestController : ControllerBase
    {
        private readonly ITypeOfRequestRepository _typeOfRequestRepository;

        public TypeOfRequestController(ITypeOfRequestRepository typeOfRequestRepository)
        {
            _typeOfRequestRepository = typeOfRequestRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllTypeOfRequests()
        {
            var typesOfRequests = await _typeOfRequestRepository.GetAll();
            var typeOfRequestDtos = typesOfRequests.Select(r => new TypeOfRequestDTO
            {
                Id = r.Id,
                Description = r.Description 
            }).ToList();

            return Ok(typeOfRequestDtos);
        }
    }
}

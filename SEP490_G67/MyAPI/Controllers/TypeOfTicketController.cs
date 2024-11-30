using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.TypeOfDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeOfTicketController : ControllerBase
    {
        private readonly ITypeOfTicketRepository _typeOfTicketRepository;

        public TypeOfTicketController(ITypeOfTicketRepository typeOfTicketRepository)
        {
            _typeOfTicketRepository = typeOfTicketRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllTypeOfTickets()
        {
            var typesOfTickets = await _typeOfTicketRepository.GetAll();
            var typeOfTicketDtos = typesOfTickets.Select(r => new TypeOfTicketDTO
            {
                Id = r.Id,
                Description = r.Description 
            }).ToList();

            return Ok(typeOfTicketDtos);
        }
    }
}

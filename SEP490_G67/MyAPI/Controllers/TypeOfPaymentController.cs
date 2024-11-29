using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.PaymentDTOs;
using MyAPI.DTOs.TypeOfDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeOfPaymentController : ControllerBase
    {
        private readonly ITypeOfPaymentRepository _typeOfPaymentRepository;

        public TypeOfPaymentController(ITypeOfPaymentRepository typeOfPaymentRepository)
        {
            _typeOfPaymentRepository = typeOfPaymentRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllTypeOfPayments()
        {
            var typesOfPayments = await _typeOfPaymentRepository.GetAll();
            var typeOfPaymentDtos = typesOfPayments.Select(r => new TypeOfPaymentDTO
            {
                Id = r.Id,
                TypeOfPayment1 = r.TypeOfPayment1
            }).ToList();

            return Ok(typeOfPaymentDtos);
        }
    }
}

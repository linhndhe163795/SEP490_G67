using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;

        public RequestController(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _requestRepository.GetAll();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var request = await _requestRepository.Get(id);
            if (request == null)
                return NotFound();
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(RequestDTO requestDto)
        {
            // Chuyển đổi RequestDTO sang thực thể Request
            var request = new Request
            {
                UserId = requestDto.UserId,
                TypeId = requestDto.TypeId,
                Status = requestDto.Status,
                Description = requestDto.Description,
                Note = requestDto.Note,
                CreatedAt = DateTime.UtcNow 
            };

            var createdRequest = await _requestRepository.CreateRequestAsync(request);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.UserId }, createdRequest);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, RequestDTO requestDto)
        {
            if (id != requestDto.UserId)
                return BadRequest();

            
            var request = new Request
            {
                UserId = requestDto.UserId,
                TypeId = requestDto.TypeId,
                Status = requestDto.Status,
                Description = requestDto.Description,
                Note = requestDto.Note,
                CreatedAt = requestDto.CreatedAt ?? DateTime.UtcNow 
            };

            var updatedRequest = await _requestRepository.UpdateRequestAsync(id, request);
            return Ok(updatedRequest);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _requestRepository.Get(id);
            if (request == null)
                return NotFound();

            await _requestRepository.Delete(request);
            return NoContent();
        }
    }
}

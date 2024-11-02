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
            var requests = await _requestRepository.GetAllRequestsWithDetailsAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var request = await _requestRepository.GetRequestWithDetailsByIdAsync(id);
            if (request == null)
                return NotFound();
            return Ok(request);
        }

        [HttpPost("/api/requestForRentCar")]
        public async Task<IActionResult> CreateRequestWithDetails(RequestDTO requestWithDetailsDto)
        {
            var createdRequest = await _requestRepository.CreateRequestRentCarAsync(requestWithDetailsDto);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.Id }, createdRequest);
        }

        [HttpPut("/api/requestForRentCar{id}")]
        public async Task<IActionResult> UpdateRequest(int id, RequestDTO requestDto)
        {
            var updated = await _requestRepository.UpdateRequestRentCarAsync(id, requestDto);
            if (updated == null)
            {
                return NotFound();
            }
            return NoContent();
        }





        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            
            var request = await _requestRepository.GetRequestWithDetailsByIdAsync(id);
            if (request == null)
                return NotFound();

           
            foreach (var detail in request.RequestDetails)
            {
                await _requestRepository.DeleteRequestDetailAsync(request.Id, detail.DetailId);
            }

           
            await _requestRepository.Delete(request);

            return NoContent(); 
        }

        [HttpPost("accept/{id}")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            try
            {
                await _requestRepository.AcceptRequestAsync(id);
                return Ok("Request accepted.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deny/{id}")]
        public async Task<IActionResult> DenyRequest(int id)
        {
            try
            {
                await _requestRepository.DenyRequestAsync(id);
                return Ok("Request denied.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}

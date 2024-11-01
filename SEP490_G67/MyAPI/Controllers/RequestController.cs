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

        [HttpPost]
        public async Task<IActionResult> CreateRequestWithDetails(RequestDTO requestWithDetailsDto)
        {
            var request = new Request
            {
                UserId = requestWithDetailsDto.UserId,
                TypeId = requestWithDetailsDto.TypeId,
                Status = requestWithDetailsDto.Status,
                Description = requestWithDetailsDto.Description,
                Note = requestWithDetailsDto.Note,
                CreatedAt = requestWithDetailsDto.CreatedAt ?? DateTime.UtcNow
            };

            var requestDetails = requestWithDetailsDto.RequestDetails.Select(detailDto => new RequestDetail
            {
                VehicleId = detailDto.VehicleId,
                StartLocation = detailDto.StartLocation,
                EndLocation = detailDto.EndLocation,
                StartTime = detailDto.StartTime,
                EndTime = detailDto.EndTime,
                Seats = detailDto.Seats
            }).ToList();

            var createdRequest = await _requestRepository.CreateRequestAsync(request, requestDetails);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.Id }, createdRequest);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequest(int id, RequestDTO requestDto)
        {
            

            
            var existingRequest = await _requestRepository.GetRequestWithDetailsByIdAsync(id);
            if (existingRequest == null)
                return NotFound();

            
            existingRequest.UserId = requestDto.UserId;
            existingRequest.TypeId = requestDto.TypeId;
            existingRequest.Status = requestDto.Status;
            existingRequest.Description = requestDto.Description;
            existingRequest.Note = requestDto.Note;
            existingRequest.CreatedAt = requestDto.CreatedAt ?? DateTime.UtcNow;

            
            var updatedDetails = requestDto.RequestDetails.Select(detailDto => new RequestDetail
            {
                VehicleId = detailDto.VehicleId,
                StartLocation = detailDto.StartLocation,
                EndLocation = detailDto.EndLocation,
                StartTime = detailDto.StartTime,
                EndTime = detailDto.EndTime,
                Seats = detailDto.Seats
            }).ToList();

            
            await _requestRepository.UpdateRequestAsync(id, existingRequest, updatedDetails);

            return Ok(existingRequest);
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


    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class RequestDetailController : ControllerBase
    {
        private readonly IRequestDetailRepository _requestDetailRepository;

        public RequestDetailController(IRequestDetailRepository requestDetailRepository)
        {
            _requestDetailRepository = requestDetailRepository;
        }
        [Authorize(Roles = "Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllRequestDetails()
        {
            var requestDetails = await _requestDetailRepository.GetAll();
            return Ok(requestDetails);
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestDetailById(int id)
        {
            var requestDetail = await _requestDetailRepository.Get(id);
            if (requestDetail == null)
                return NotFound();
            return Ok(requestDetail);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequestDetail(RequestDetailDTO requestDetailDto)
        {
            var createdRequestDetail = await _requestDetailRepository.CreateRequestDetailAsync(requestDetailDto);
            return CreatedAtAction(nameof(GetRequestDetailById), new { id = createdRequestDetail.RequestId }, createdRequestDetail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRequestDetail(int id, RequestDetailDTO requestDetailDto)
        {
            if (id != requestDetailDto.RequestId)
                return BadRequest();

            var updatedRequestDetail = await _requestDetailRepository.UpdateRequestDetailAsync(id, requestDetailDto);
            return Ok(updatedRequestDetail);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequestDetail(int id)
        {
            var requestDetail = await _requestDetailRepository.Get(id);
            if (requestDetail == null)
                return NotFound();

            await _requestDetailRepository.Delete(requestDetail);
            return NoContent();
        }
    }
}

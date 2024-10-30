using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.ReviewDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly GetInforFromToken _getInforFromToken;

        public ReviewController(IReviewRepository reviewRepository, GetInforFromToken getInforFromToken)
        {
            _reviewRepository = reviewRepository;
            _getInforFromToken = getInforFromToken;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewRepository.GetAll();
            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewRepository.Get(id);
            if (review == null)
            {
                return NotFound();
            }
            return Ok(review);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var userId = _getInforFromToken.GetIdInHeader(token);

            // Gán userId vào reviewDto
            reviewDto.UserId = userId;

            var review = await _reviewRepository.CreateReviewAsync(reviewDto);
            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy token từ header Authorization
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var userId = _getInforFromToken.GetIdInHeader(token);

            // Gán userId vào reviewDto
            reviewDto.UserId = userId;

            var review = await _reviewRepository.UpdateReviewAsync(id, reviewDto);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _reviewRepository.Get(id);
            if (review == null)
            {
                return NotFound();
            }

            await _reviewRepository.Delete(review);
            return NoContent();
        }
    }
}

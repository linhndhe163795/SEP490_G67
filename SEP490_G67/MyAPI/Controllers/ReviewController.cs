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
        [Authorize(Roles = "Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewRepository.GetAll();
            return Ok(reviews);
        }
        [Authorize(Roles = "Staff")]
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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var userId = _getInforFromToken.GetIdInHeader(token);

            
            

            var review = await _reviewRepository.CreateReviewAsync(reviewDto, userId);
            return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewDTO reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var userId = _getInforFromToken.GetIdInHeader(token);

            
            

            var review = await _reviewRepository.UpdateReviewAsync(id, reviewDto, userId);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }

        [Authorize]
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

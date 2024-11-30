using MyAPI.DTOs.ReviewDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyAPI.Repositories.Impls;
using System;
using MyAPI.Helper;

namespace MyAPI.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly SEP490_G67Context _context;
        IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;

        public ReviewRepository(SEP490_G67Context context, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper
) : base(context)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
        }

        

        public async Task<Review> CreateReviewAsync(ReviewDTO reviewDto)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            var review = new Review
            {
                Description = reviewDto.Description,
                UserId = userId,
                TripId = reviewDto.TripId,
                CreatedAt = reviewDto.CreatedAt ?? DateTime.Now,
                CreatedBy = reviewDto.CreatedBy,
                UpdateAt = reviewDto.UpdateAt,
                UpdateBy = reviewDto.UpdateBy
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return review;
        }

        public async Task<Review> UpdateReviewAsync(int id, ReviewDTO reviewDto)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return null;
            }

            review.Description = reviewDto.Description;
            review.UserId = userId;
            review.TripId = reviewDto.TripId;
            review.UpdateAt = DateTime.Now;
            review.UpdateBy = reviewDto.UpdateBy;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return review;
        }

       
    }
}

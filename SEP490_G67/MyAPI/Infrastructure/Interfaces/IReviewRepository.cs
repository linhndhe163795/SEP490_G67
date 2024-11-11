using MyAPI.DTOs.ReviewDTOs;
using MyAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        
        Task<Review> CreateReviewAsync(ReviewDTO reviewDto, int userId);
        Task<Review> UpdateReviewAsync(int id, ReviewDTO reviewDto, int userId);
        
    }
}

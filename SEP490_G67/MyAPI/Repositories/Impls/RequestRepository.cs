using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {

        public RequestRepository(SEP490_G67Context context) : base(context)
        {
        }

        public async Task<Request> CreateRequestAsync(RequestDTO request)
        {

            var requestAdd = new Request
            {
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                TypeId = request.TypeId,
                Status = false,
                Description = request.Description,
                Note = request.Note,
            };

            _context.Requests.Add(requestAdd);
            await _context.SaveChangesAsync();
            
            return requestAdd;
        }

        public async Task<Request> UpdateRequestAsync(int id, Request request)
        {
            var existingRequest = await Get(id);
            if (existingRequest == null)
            {
                throw new KeyNotFoundException("Request not found");
            }

            // Cập nhật các trường cần thiết
            existingRequest.Status = request.Status;
            existingRequest.Description = request.Description;
            existingRequest.Note = request.Note;
            existingRequest.CreatedAt = request.CreatedAt ?? existingRequest.CreatedAt;

            await Update(existingRequest);
            return existingRequest;
        }
    }
}

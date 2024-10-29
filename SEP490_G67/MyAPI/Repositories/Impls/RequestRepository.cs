using Microsoft.EntityFrameworkCore;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {
        private readonly SEP490_G67Context _context;

        public RequestRepository(SEP490_G67Context context) : base(context)
        {
            _context = context;
        }

        public async Task<Request> CreateRequestAsync(Request request)
        {
            request.CreatedAt = DateTime.UtcNow;  
            await Add(request);
            return request;
        }

        public async Task<Request> UpdateRequestAsync(int id, Request request)
        {
            var existingRequest = await Get(id);
            if (existingRequest == null)
            {
                throw new KeyNotFoundException("Request not found");
            }

            // Cập nhật các trường cần thiết
            existingRequest.UserId = request.UserId;
            existingRequest.TypeId = request.TypeId;
            existingRequest.Status = request.Status;
            existingRequest.Description = request.Description;
            existingRequest.Note = request.Note;
            existingRequest.CreatedAt = request.CreatedAt ?? existingRequest.CreatedAt;

            await Update(existingRequest);
            return existingRequest;
        }
    }
}

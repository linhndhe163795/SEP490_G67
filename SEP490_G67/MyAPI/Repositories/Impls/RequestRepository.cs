
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

        public async Task<Request> CreateRequestAsync(Request request, List<RequestDetail> requestDetails)
        {
            // Thêm Request mới vào CSDL
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();

            // Thêm danh sách RequestDetail và thiết lập RequestId
            foreach (var detail in requestDetails)
            {
                detail.RequestId = request.Id;
                _context.RequestDetails.Add(detail);
            }

            await _context.SaveChangesAsync();
            return request;

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

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
        }


        public async Task<Request> UpdateRequestAsync(int id, Request request, List<RequestDetail> requestDetails)
        {
            var existingRequest = await GetRequestWithDetailsByIdAsync(id);
            if (existingRequest == null)
            {
                throw new KeyNotFoundException("Request not found");
            }

            // Cập nhật các trường cần thiết của yêu cầu
            existingRequest.UserId = request.UserId;
            existingRequest.TypeId = request.TypeId;
            existingRequest.Status = request.Status;
            existingRequest.Description = request.Description;
            existingRequest.Note = request.Note;
            existingRequest.CreatedAt = request.CreatedAt ?? existingRequest.CreatedAt;

            // Cập nhật chi tiết yêu cầu
            // Xóa các chi tiết không còn cần thiết
            var existingDetails = existingRequest.RequestDetails.ToList(); // Lưu chi tiết cũ

            foreach (var detail in existingDetails)
            {
                var updatedDetail = requestDetails.FirstOrDefault(d => d.VehicleId == detail.VehicleId);
                if (updatedDetail == null)
                {
                    // Nếu chi tiết không còn trong danh sách mới, xóa nó
                    _context.RequestDetails.Remove(detail);
                }
                else
                {
                    // Nếu chi tiết đã tồn tại, cập nhật thông tin
                    detail.StartLocation = updatedDetail.StartLocation;
                    detail.EndLocation = updatedDetail.EndLocation;
                    detail.StartTime = updatedDetail.StartTime;
                    detail.EndTime = updatedDetail.EndTime;
                    detail.Seats = updatedDetail.Seats;
                }
            }

            // Thêm các chi tiết mới
            foreach (var detail in requestDetails)
            {
                if (!existingDetails.Any(d => d.VehicleId == detail.VehicleId))
                {
                    detail.RequestId = existingRequest.Id; // Gán ID yêu cầu cho chi tiết mới
                    _context.RequestDetails.Add(detail);
                }
            }

            // Cập nhật yêu cầu và lưu vào CSDL
            await _context.SaveChangesAsync();
            return existingRequest;
        }


        public async Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync()
        {
            return await _context.Requests
                .Include(r => r.RequestDetails) // Bao gồm chi tiết của request
                .ToListAsync();
        }

        public async Task<Request> GetRequestWithDetailsByIdAsync(int id)
        {
            return await _context.Requests
                .Include(r => r.RequestDetails) // Bao gồm chi tiết của request
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteRequestDetailAsync(int requestId, int detailId)
        {
            var detail = await _context.RequestDetails
                .FirstOrDefaultAsync(d => d.RequestId == requestId && d.DetailId == detailId);

            if (detail != null)
            {
                _context.RequestDetails.Remove(detail);
                await _context.SaveChangesAsync();
            }
        }

    }
}

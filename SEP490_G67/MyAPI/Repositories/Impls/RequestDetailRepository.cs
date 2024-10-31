using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class RequestDetailRepository : GenericRepository<RequestDetail>, IRequestDetailRepository
    {
        public RequestDetailRepository(SEP490_G67Context context) : base(context)
        {
        }

        public async Task<RequestDetail> CreateRequestDetailAsync(RequestDetailDTO requestDetailDto)
        {
            var requestDetail = new RequestDetail
            {
                RequestId = requestDetailDto.RequestId,
                VehicleId = requestDetailDto.VehicleId,
                StartLocation = requestDetailDto.StartLocation,
                EndLocation = requestDetailDto.EndLocation,
                StartTime = requestDetailDto.StartTime,
                EndTime = requestDetailDto.EndTime,
                Seats = requestDetailDto.Seats,
            };
            _context.RequestDetails.Add(requestDetail);
            await _context.SaveChangesAsync();

            return requestDetail;
        }

        public async Task<RequestDetail> UpdateRequestDetailAsync(int id, RequestDetailDTO requestDetailDto)
        {
            var existingRequestDetail = await Get(id);
            if (existingRequestDetail == null)
            {
                throw new KeyNotFoundException("RequestDetail not found");
            }

            // Cập nhật các trường cần thiết từ DTO
            existingRequestDetail.VehicleId = requestDetailDto.VehicleId;
            existingRequestDetail.StartLocation = requestDetailDto.StartLocation;
            existingRequestDetail.EndLocation = requestDetailDto.EndLocation;
            existingRequestDetail.StartTime = requestDetailDto.StartTime;
            existingRequestDetail.EndTime = requestDetailDto.EndTime;
            existingRequestDetail.Seats = requestDetailDto.Seats;
            
            await Update(existingRequestDetail);
            return existingRequestDetail;
        }
    }
}

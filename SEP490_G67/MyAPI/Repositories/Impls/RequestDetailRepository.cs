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
            var typeId = await _context.Requests
                        .Where(x => x.Id == requestDetailDto.RequestId)
                        .Select(r => r.TypeId)
                        .FirstOrDefaultAsync();


            if (requestDetailDto.RequestId <= 0)
            {
                throw new Exception("Invalid Request ID.");
            }

            if (requestDetailDto.VehicleId <= 0)
            {
                throw new Exception("Invalid Vehicle ID.");
            }

            if (requestDetailDto.StartTime >= requestDetailDto.EndTime && typeId != 1)
            {
                throw new Exception("Start time must be earlier than end time.");
            }
            if (string.IsNullOrWhiteSpace(requestDetailDto.StartLocation) && typeId != 1 )
            {
                throw new Exception("Start location is required.");
            }

            if (string.IsNullOrWhiteSpace(requestDetailDto.EndLocation) && typeId != 1)
            {
                throw new Exception("End location is required.");
            }
            if (requestDetailDto.Seats <= 0 && typeId != 1)
            {
                throw new Exception("Invalid Vehicle ID.");
            }
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
            //_context.RequestDetails.Add(requestDetail);
            //await _context.SaveChangesAsync();

            try
            {
                _context.RequestDetails.Add(requestDetail);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error saving data: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return requestDetail;
        }

        public async Task<RequestDetail> UpdateRequestDetailAsync(int id, RequestDetailDTO requestDetailDto)
        {
            try
            {
                var existingRequestDetail = await Get(id);
                if (existingRequestDetail == null)
                {
                    throw new KeyNotFoundException("RequestDetail not found");
                }

                if (id <= 0)
                {
                    throw new Exception("Invalid RequestDetail ID.");
                }

                if (requestDetailDto.VehicleId <= 0)
                {
                    throw new Exception("Invalid Vehicle ID.");
                }

                if (string.IsNullOrWhiteSpace(requestDetailDto.StartLocation))
                {
                    throw new Exception("Start location is required.");
                }

                if (string.IsNullOrWhiteSpace(requestDetailDto.EndLocation))
                {
                    throw new Exception("End location is required.");
                }

                if (requestDetailDto.StartTime >= requestDetailDto.EndTime)
                {
                    throw new Exception("Start time must be earlier than end time.");
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
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Reflection.Metadata;


namespace MyAPI.Repositories.Impls
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {
        private readonly SEP490_G67Context _context;

        public RequestRepository(SEP490_G67Context context) : base(context)
        {
            _context = context;
        }

        public async Task<Request> UpdateRequestRentCarAsync(int id, RequestDTO requestDTO)
        {
            var existingRequest = await GetRequestWithDetailsByIdAsync(id);
            if (existingRequest == null)
            {
                throw new KeyNotFoundException("Request not found");
            }

            existingRequest.UserId = requestDTO.UserId;
            existingRequest.TypeId = requestDTO.TypeId;
            existingRequest.Status = requestDTO.Status;
            existingRequest.Description = requestDTO.Description;
            existingRequest.Note = requestDTO.Note;
            existingRequest.CreatedAt = requestDTO.CreatedAt ?? existingRequest.CreatedAt;


            var existingDetails = existingRequest.RequestDetails.ToList();
            foreach (var detail in existingDetails)
            {
                var updatedDetail = requestDTO.RequestDetails.FirstOrDefault(d => d.VehicleId == detail.VehicleId);
                if (updatedDetail == null)
                {
                    _context.RequestDetails.Remove(detail);
                }
                else
                {
                    detail.StartLocation = updatedDetail.StartLocation;
                    detail.EndLocation = updatedDetail.EndLocation;
                    detail.StartTime = updatedDetail.StartTime;
                    detail.EndTime = updatedDetail.EndTime;
                    detail.Seats = updatedDetail.Seats;
                }
            }

            foreach (var detail in requestDTO.RequestDetails)
            {
                if (!existingDetails.Any(d => d.VehicleId == detail.VehicleId))
                {
                    var newDetail = new RequestDetail
                    {
                        VehicleId = detail.VehicleId,
                        StartLocation = detail.StartLocation,
                        EndLocation = detail.EndLocation,
                        StartTime = detail.StartTime,
                        EndTime = detail.EndTime,
                        Seats = detail.Seats,
                        RequestId = existingRequest.Id
                    };
                    _context.RequestDetails.Add(newDetail);
                }
            }

            await _context.SaveChangesAsync();
            return existingRequest;
        }

        public async Task<Request> CreateRequestRentCarAsync(RequestDTO requestDTO)
        {
            var newRequest = new Request
            {
                UserId = requestDTO.UserId,
                TypeId = requestDTO.TypeId,
                Status = requestDTO.Status,
                Description = requestDTO.Description,
                Note = requestDTO.Note,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            var maxId = await _context.Requests.MaxAsync(r => (int?)r.Id) ?? 0;
            var newRequestId = maxId + 1;

            foreach (var detailDto in requestDTO.RequestDetails)
            {
                var requestDetail = new RequestDetail
                {
                    VehicleId = detailDto.VehicleId,
                    StartLocation = detailDto.StartLocation,
                    EndLocation = detailDto.EndLocation,
                    StartTime = detailDto.StartTime,
                    EndTime = detailDto.EndTime,
                    Seats = detailDto.Seats,
                    RequestId = detailDto.RequestId,
                };
                await _context.RequestDetails.AddAsync(requestDetail);
            }

            await _context.SaveChangesAsync();
            return newRequest;
        }


        public async Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync()
        {
            return await _context.Requests
                .Include(r => r.RequestDetails)
                .ToListAsync();
        }

        public async Task<Request> GetRequestWithDetailsByIdAsync(int id)
        {
            return await _context.Requests
                .Include(r => r.RequestDetails)
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

        public async Task<Request> CreateRequestVehicleAsync(RequestDTO requestDTO)
        {
            var newRequest = new Request
            {
                CreatedAt = DateTime.Now,
                Description = requestDTO.Description,
                Note = requestDTO.Note,
                Status = requestDTO.Status,
                TypeId = requestDTO.TypeId,
                UserId = requestDTO.UserId,
            };

            await _context.Requests.AddAsync(newRequest);
            await _context.SaveChangesAsync();

            return newRequest;
        }

        public async Task<bool> UpdateRequestVehicleAsync(int requestId, Request request)
        {
            var update = await _context.Requests.SingleOrDefaultAsync(s => s.Id == requestId);
            if (update != null)
            {
                update.Status = request.Status;
                update.Note = request.Note;

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> AcceptRequestAsync(int requestId)
        {
            var request = await GetRequestWithDetailsByIdAsync(requestId);
            if (request == null)
            {
                throw new KeyNotFoundException("Request not found");
            }
            request.Status = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DenyRequestAsync(int requestId)
        {
            var request = await GetRequestWithDetailsByIdAsync(requestId);
            if (request == null)
            {
                throw new KeyNotFoundException("Request not found");
            }
            request.Status = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task createRequestCancleTicket(RequestCancleTicketDTOs requestCancleTicketDTOs, int userId)
        {
            try
            {
                DateTime dateTimeCancle = DateTime.Now.AddHours(-2);
                var listTicketId = await _context.Tickets.Where(x => x.UserId == userId && x.TimeFrom <= dateTimeCancle).ToListAsync();
                if (!listTicketId.Any()) 
                {
                    throw new NullReferenceException("Không có vé của nào của user");
                }
                else
                {
                    foreach (var ticket in listTicketId)
                    {
                        if (!ticket.Id.Equals(requestCancleTicketDTOs.TicketId))
                        {
                            throw new NullReferenceException("Không có vé hợp lệ để hủy");
                        }
                    }
                    var RequestCancleTicket = new Request
                    {
                        UserId = userId,
                        TypeId = Helper.Constant.HUY_VE,
                        Description = "Yêu cầu hủy vé xe",
                    };
                    _context.Requests.Add(RequestCancleTicket);
                    await _context.SaveChangesAsync();
                    var RequestCancleTicketDetails = new RequestDetail
                    {
                        RequestId = RequestCancleTicket.Id,
                        TicketId = requestCancleTicketDTOs.TicketId,
                    };
                    _context.RequestDetails.Add(RequestCancleTicketDetails);
                    await _context.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}

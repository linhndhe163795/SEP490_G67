﻿using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MyAPI.DTOs;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Reflection.Metadata;
using Constant = MyAPI.Helper.Constant;

namespace MyAPI.Repositories.Impls
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {

        public RequestRepository(SEP490_G67Context _context) : base(_context)
        {
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
                CreatedAt = DateTime.UtcNow,
                CreatedBy = requestDTO.CreatedBy,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Constant.ADMIN,
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
                    TicketId = detailDto.TicketId,
                    StartLocation = detailDto.StartLocation,
                    EndLocation = detailDto.EndLocation,
                    StartTime = detailDto.StartTime,
                    EndTime = detailDto.EndTime,
                    Seats = detailDto.Seats,
                    RequestId = maxId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = requestDTO.CreatedBy,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = Constant.ADMIN,
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
                var ticketToCancel = listTicketId.FirstOrDefault(ticket => ticket.Id == requestCancleTicketDTOs.TicketId);
                if (ticketToCancel == null)
                {
                    throw new NullReferenceException("Không có vé hợp lệ để hủy");
                }
                var RequestCancleTicket = new Request
                {
                    UserId = userId,
                    TypeId = Helper.Constant.HUY_VE,
                    Description = requestCancleTicketDTOs.Description,
                    Note = "Chờ xác nhận",
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                };
                _context.Requests.Add(RequestCancleTicket);
                await _context.SaveChangesAsync();
                var RequestCancleTicketDetails = new RequestDetail
                {
                    RequestId = RequestCancleTicket.Id,
                    TicketId = requestCancleTicketDTOs.TicketId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };
                _context.RequestDetails.Add(RequestCancleTicketDetails);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ResponeCancleTicketDTOs>> getListRequestCancle()
        {
            try
            {
                var listRequestCancleTicket = await (from r in _context.Requests
                                                     join rd in _context.RequestDetails
                                                     on r.Id equals rd.RequestId
                                                     where r.TypeId == Helper.Constant.HUY_VE
                                                     select new ResponeCancleTicketDTOs
                                                     {
                                                         Description = r.Description,
                                                         TicketId = rd.TicketId,
                                                     }).ToListAsync();
                if (listRequestCancleTicket == null)
                {
                    throw new NullReferenceException();
                }
                return listRequestCancleTicket;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task updateStatusRequestCancleTicket(int requestId, int staffId)
        {
            try
            {
                var requestCancleTicket = await _context.Requests.FirstOrDefaultAsync(x => x.Id == requestId);
                if (requestCancleTicket == null)
                {
                    throw new NullReferenceException();
                }
                requestCancleTicket.Status = true;
                requestCancleTicket.Note = "Đã xác nhận";
                var getTicketCancle = await (from r in _context.Requests
                                             join rd in _context.RequestDetails
                                             on r.Id equals rd.RequestId
                                             where r.Id == requestId
                                             select rd
                                               ).FirstOrDefaultAsync();
                if (getTicketCancle == null)
                {
                    throw new NullReferenceException();
                }
                var inforTicketCancle = await (from t in _context.Tickets
                                               join p in _context.Payments
                                               on t.Id equals p.TicketId
                                               join rd in _context.RequestDetails on t.Id equals rd.TicketId
                                               join r in _context.Requests on rd.RequestId equals r.Id
                                               join u in _context.Users on t.UserId equals u.Id
                                               where r.Id == requestId && t.Id == getTicketCancle.TicketId
                                               select new { t, p, u, r }
                                               ).FirstOrDefaultAsync();
                if (inforTicketCancle == null)
                {
                    throw new Exception("Không có vé để hủy");
                }
                var pointOfPayment = (int)inforTicketCancle.t.Price * Helper.Constant.TICH_DIEM;

                var updatePointUserCancle = await (from pu in _context.PointUsers
                                                   where pu.Id == (from innerPu in _context.PointUsers
                                                                   where innerPu.UserId == inforTicketCancle.t.UserId
                                                                   select innerPu.Id).Max()
                                                   && pu.UserId == inforTicketCancle.t.UserId
                                                   select pu
                                  ).FirstOrDefaultAsync();
                if (updatePointUserCancle == null)
                {
                    throw new Exception();
                }
                if (updatePointUserCancle.Points <= pointOfPayment)
                {
                    var PointUserMinus = new PointUser
                    {
                        PaymentId = inforTicketCancle.p.PaymentId,
                        UserId = inforTicketCancle.t.UserId,
                        Points = 0,
                        PointsMinus = (int)pointOfPayment,
                        Date = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        CreatedBy = inforTicketCancle.t.UserId,
                        UpdateAt = null,
                        UpdateBy = null
                    };
                    _context.PointUsers.Add(PointUserMinus);
                }
                else
                {
                    var PointUserMinus = new PointUser
                    {
                        PaymentId = inforTicketCancle.p.PaymentId,
                        UserId = inforTicketCancle.t.UserId,
                        Points = (int)(updatePointUserCancle.Points - pointOfPayment),
                        PointsMinus = (int)pointOfPayment,
                        Date = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        CreatedBy = inforTicketCancle.t.UserId,
                        UpdateAt = null,
                        UpdateBy = null
                    };
                    _context.PointUsers.Add(PointUserMinus);
                }
                inforTicketCancle.t.Price = 0;
                inforTicketCancle.t.Status = "Hủy vé";
                inforTicketCancle.p.Price = 0;
                var UserCancleTicket = new UserCancleTicket
                {
                    PaymentId = inforTicketCancle.p.PaymentId,
                    ReasonCancle = inforTicketCancle.r.Description,
                    UserId = inforTicketCancle.r.UserId,
                    TicketId = inforTicketCancle.t.Id,
                    CreatedAt = DateTime.Now,
                    CreatedBy = staffId,
                    UpdateAt = null,
                    UpdateBy = null
                };
                _context.UserCancleTickets.Add(UserCancleTicket);
                SendMailDTO sendMailDTO = new()
                {
                    FromEmail = "duclinh5122002@gmail.com",
                    Password = "jetj haze ijdw euci",
                    ToEmail = inforTicketCancle.u.Email,
                    Subject = "Xác nhận hủy vé",
                    Body = "Hệ thống đã xác nhận hủy vé xe chuyến đi: " + inforTicketCancle.t.PointStart + " - " + inforTicketCancle.t.PointEnd,
                };

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}

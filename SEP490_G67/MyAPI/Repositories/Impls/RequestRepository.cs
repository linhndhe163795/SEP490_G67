using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MyAPI.DTOs;
using MyAPI.DTOs.HistoryRentDriverDTOs;
using MyAPI.DTOs.HistoryRentVehicle;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Reflection.Metadata;
using Constant = MyAPI.Helper.Constant;

namespace MyAPI.Repositories.Impls
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly IRequestDetailRepository _requestDetailRepository;

        public RequestRepository(SEP490_G67Context _context, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper, IRequestDetailRepository requestDetailRepository) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _requestDetailRepository = requestDetailRepository;
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
                UpdateAt = DateTime.UtcNow,
                UpdateBy = Constant.ADMIN,
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
                    Price = detailDto.Price,
                    CreatedAt = DateTime.UtcNow,
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
                update.UpdateAt = DateTime.Now;

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
                var inforTicketCancle = await (from t in _context.Tickets join p in _context.Payments
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
                var pointOfPayment = (int) inforTicketCancle.t.Price * Helper.Constant.TICH_DIEM;

                var updatePointUserCancle = await (from pu in _context.PointUsers
                                                   where pu.Id == (from innerPu in _context.PointUsers
                                                                   where innerPu.UserId == inforTicketCancle.t.UserId
                                                                   select innerPu.Id).Max()
                                                   && pu.UserId == inforTicketCancle.t.UserId
                                                   select pu
                                  ).FirstOrDefaultAsync();
                if(updatePointUserCancle == null)
                {
                    throw new Exception();
                }
                if(updatePointUserCancle.Points <= pointOfPayment)
                {
                    var PointUserMinus = new PointUser
                    {
                        PaymentId = inforTicketCancle.p.PaymentId,
                        UserId = inforTicketCancle.t.UserId,
                        Points = 0,
                        PointsMinus = (int) pointOfPayment,
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
                        Points = (int) (updatePointUserCancle.Points - pointOfPayment),
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

        public async Task<bool> CreateRequestRentVehicleAsync(RentVehicleAddDTO rentVehicleAddDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var addRentVehicle = new Request
                {
                    UserId = userId,
                    TypeId = 4,
                    Status = false,
                    Description = "Yêu cầu thuê xe tiện chuyến",
                    CreatedAt = DateTime.Now,
                    Note = "Chờ xác nhận",
                    CreatedBy = userId,
                    UpdateAt = DateTime.Now,
                    UpdateBy = userId,
                };

                await _context.Requests.AddAsync(addRentVehicle);
                await _context.SaveChangesAsync();

                var addRentVehicelRequestDetails = new RequestDetail
                {
                    RequestId = addRentVehicle.Id,
                    VehicleId = null,
                    TicketId = null,
                    StartLocation = rentVehicleAddDTO?.StartLocation,
                    EndLocation = rentVehicleAddDTO?.EndLocation,
                    StartTime = rentVehicleAddDTO?.StartTime,
                    EndTime = rentVehicleAddDTO?.EndTime,
                    Seats = rentVehicleAddDTO?.Seats,
                    Price = rentVehicleAddDTO?.Price,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    UpdateAt = DateTime.Now,
                    UpdateBy = userId,
                };

                await _context.RequestDetails.AddAsync(addRentVehicelRequestDetails);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

        public async Task<bool> CreateRequestRentDriverAsync(RequestDetailForRentDriver rentDriverAddDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                //var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                //int userId = _tokenHelper.GetIdInHeader(token);

                //if (userId == -1)
                //{
                //    throw new Exception("Invalid user ID from token.");
                //}

                //var addRentDriver = new Request
                //{
                //    UserId = userId,
                //    TypeId = 5,
                //    Status = false,
                //    Description = "Yêu cầu thuê tài xế",
                //    CreatedAt = DateTime.Now,
                //    Note = "Chờ xác nhận",
                //    CreatedBy = userId,
                //    UpdateAt = DateTime.Now,
                //    UpdateBy = userId,
                //};

                //await _context.Requests.AddAsync(addRentDriver);
                //await _context.SaveChangesAsync();

                //var addRentDriverRequestDetails = new RequestDetail
                //{
                //    RequestId = addRentDriver.Id,
                //    DriverId = rentDriverAddDTO?.DriverId,
                //    VehicleId = null,
                //    TicketId = null,
                //    StartLocation = rentDriverAddDTO?.StartLocation,
                //    EndLocation = rentDriverAddDTO?.EndLocation,
                //    StartTime = rentDriverAddDTO?.StartTime,
                //    EndTime = rentDriverAddDTO?.EndTime,
                //    Seats = rentDriverAddDTO?.Seats,
                //    Price = rentDriverAddDTO?.Price,
                //    CreatedAt = DateTime.Now,
                //    CreatedBy = userId,
                //    UpdateAt = DateTime.Now,
                //    UpdateBy = userId,
                //};

                //await _context.RequestDetails.AddAsync(addRentDriverRequestDetails);
                //await _context.SaveChangesAsync();

                //await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error in CreateRequestRentDriverAsync: {ex.Message}");
            }
        }


        public async Task<bool> CreateRequestCovenient(ConvenientTripDTO convenientTripDTO)
        {
            // Lấy User ID từ token
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new UnauthorizedAccessException("Invalid user ID from token.");
            }

            int type_Of_Trip;
            string descriptionType = "";
            if (convenientTripDTO.TypeOfTrip == 2)
            {
                type_Of_Trip = 5;
                descriptionType = "Yêu cầu đặt vé xe ghép";
            }
            else if (convenientTripDTO.TypeOfTrip == 3)
            {
                type_Of_Trip = 6;
                descriptionType = "Yêu cầu bao xe ghép";
            }
            else
            {
                throw new ArgumentException("Invalid TypeOfTrip. Only values 2 or 3 are allowed.");
            }

            if (string.IsNullOrEmpty(convenientTripDTO.PointStart) || string.IsNullOrEmpty(convenientTripDTO.PointEnd))
            {
                throw new ArgumentException("PointStart and PointEnd cannot be null or empty.");
            }

            if (convenientTripDTO.Price <= 0)
            {
                throw new ArgumentException("Price must be greater than 0.");
            }

            if (string.IsNullOrEmpty(convenientTripDTO.UserName) || string.IsNullOrEmpty(convenientTripDTO.PhoneNumber))
            {
                throw new ArgumentException("UserName and PhoneNumber cannot be null or empty.");
            }

            var addConvenientRequest = new Request
            {
                UserId = userId,
                TypeId = type_Of_Trip,
                Status = false,
                Description = descriptionType,
                Note = "Đang chờ xác nhận",
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                UpdateAt = DateTime.Now,
                UpdateBy = userId,
            };

            await _context.Requests.AddAsync(addConvenientRequest);

            await _context.SaveChangesAsync();

            var addConvenientRequest_Details = new RequestDetail
            {
                RequestId = addConvenientRequest.Id,
                VehicleId = null,
                TicketId = null,
                StartLocation = convenientTripDTO.PointStart,
                EndLocation = convenientTripDTO.PointEnd,
                StartTime = convenientTripDTO.StartTime,
                EndTime = null,
                Seats = null,
                Price = convenientTripDTO.Price,
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                UpdateAt = DateTime.Now,
                UpdateBy = userId,
                UserName = convenientTripDTO.UserName,
                PhoneNumber = convenientTripDTO.PhoneNumber,
            };

            await _context.RequestDetails.AddAsync(addConvenientRequest_Details);

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> UpdateStatusRequestConvenient(int requestId, bool choose)
        {
            var checkRequest = await _context.Requests.FirstOrDefaultAsync(s => s.Id == requestId);
            if (checkRequest == null)
            {
                throw new Exception("RequestId not found");
            }

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);
            if (userId == -1)
            {
                throw new UnauthorizedAccessException("Invalid user ID from token.");
            }

            checkRequest.Note = choose ? "Đã xác nhận" : "Từ chối xác nhận";
            checkRequest.Status = choose;

            await UpdateRequestVehicleAsync(requestId, checkRequest);

            var updateRequestDetail = new RequestDetailDTO
            {
                UpdatedAt = DateTime.Now,
                UpdatedBy = userId,
                EndTime = DateTime.Now,
            };

            var updateRequestDetailRentVehicle = await _requestDetailRepository.CreateRequestDetailAsync(updateRequestDetail);
            if (updateRequestDetailRentVehicle == null)
            {
                throw new Exception("Failed to create request detail.");
            }

            if(!choose)
            {
                return true;   
            }

            var checkRequestDetail = await _context.RequestDetails.SingleOrDefaultAsync(s => s.RequestId == requestId);
            if (checkRequestDetail == null)
            {
                throw new Exception("Request detail not found for the given requestId.");
            }

            var tripId = await _context.Trips
                .Where(t => t.PointStart.Contains(checkRequestDetail.StartLocation) && t.PointEnd.Contains(checkRequestDetail.EndLocation))
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (tripId == 0)
            {
                throw new Exception("TripId not found for the given start and end locations.");
            }

            int typeOfTrip;
            if (checkRequest.TypeId == 5)
            {
                typeOfTrip = 2;
            }
            else if (checkRequest.TypeId == 6)
            {
                typeOfTrip = 4;
            }
            else
            {
                throw new Exception("Invalid TypeId for determining TypeOfTrip.");
            }

            var addTicket = new Ticket
            {
                Price = checkRequestDetail.Price,
                CodePromotion = null,
                PricePromotion = null,
                NumberTicket = 2,
                PointEnd = checkRequestDetail.EndLocation,
                PointStart = checkRequestDetail.StartLocation,
                TimeFrom = checkRequestDetail.StartTime,
                TimeTo = DateTime.Now,
                UserId = checkRequest.UserId,
                VehicleId = checkRequestDetail.VehicleId,
                TripId = tripId,
                TypeOfTicket = typeOfTrip,
                TypeOfPayment = 2,
                Status = "Thanh toán bằng tiền mặt",
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                UpdateAt = DateTime.Now,
                UpdateBy = userId,
            };

            await _context.Tickets.AddAsync(addTicket);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}

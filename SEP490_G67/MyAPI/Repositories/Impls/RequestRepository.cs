using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MyAPI.DTOs;
using MyAPI.DTOs.HistoryRentVehicle;
using MyAPI.DTOs.PaymentDTOs;
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
        private readonly IPromotionUserRepository _promotionUserRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public RequestRepository(SEP490_G67Context _context, IHttpContextAccessor httpContextAccessor
            , GetInforFromToken tokenHelper, IRequestDetailRepository requestDetailRepository
            , IPromotionUserRepository promotionUserRepository, IPaymentRepository paymentRepository,
            IMapper mapper) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _requestDetailRepository = requestDetailRepository;
            _promotionUserRepository = promotionUserRepository;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<RequestDetailDTO> GetRequestDetailByIdAsync(int requestId)
        {
            var requestDetail = await _context.RequestDetails
                .Where(rd => rd.RequestId == requestId)
                .Select(rd => new RequestDetailDTO
                {
                    RequestId = rd.RequestId,
                    TicketId = rd.TicketId,
                    VehicleId = rd.VehicleId,
                    DriverId = rd.DriverId,
                    StartLocation = rd.StartLocation,
                    EndLocation = rd.EndLocation,
                    StartTime = rd.StartTime,
                    EndTime = rd.EndTime,
                    Seats = rd.Seats,
                    CreatedAt = rd.CreatedAt,
                    CreatedBy = rd.CreatedBy,
                    UpdatedAt = rd.UpdateAt,
                    UpdatedBy = rd.UpdateBy,
                    Price = rd.Price,
                })
                .FirstOrDefaultAsync();

            if (requestDetail == null)
            {
                throw new KeyNotFoundException($"RequestDetail with RequestId {requestId} not found.");
            }

            return requestDetail;
        }

        public async Task<bool> UpdateRequestRentCarAsync(int requestId, RequestDTOForRentCar rentVehicleAddDTO)
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


                var existingRequest = await _context.Requests.FindAsync(requestId);
                if (existingRequest == null)
                {
                    throw new Exception("Request not found.");
                }


                if (existingRequest.UserId != userId)
                {
                    throw new Exception("You do not have permission to update this request.");
                }


                existingRequest.Status = false;
                existingRequest.Description = "Yêu cầu thuê xe du lịch ";
                existingRequest.Note = "Chờ xác nhận ";
                existingRequest.UpdateAt = DateTime.Now;
                existingRequest.UpdateBy = userId;

                _context.Requests.Update(existingRequest);


                var existingRequestDetail = await _context.RequestDetails
                    .FirstOrDefaultAsync(rd => rd.RequestId == requestId);

                if (existingRequestDetail == null)
                {
                    throw new Exception("Request detail not found.");
                }

                if (string.IsNullOrWhiteSpace(rentVehicleAddDTO.StartLocation))
                {
                    throw new Exception("Start location is required.");
                }
                if (rentVehicleAddDTO.StartLocation.Length > 255)
                {
                    throw new Exception("Start location cannot exceed 255 characters.");
                }
                if (rentVehicleAddDTO.StartTime == DateTime.MinValue || rentVehicleAddDTO.EndTime == DateTime.MinValue)
                {
                    throw new Exception("Start time and end time are required.");
                }
                if (rentVehicleAddDTO.StartTime >= rentVehicleAddDTO.EndTime)
                {
                    throw new Exception("Start time must be earlier than end time.");
                }
                if (rentVehicleAddDTO.Seats <= 0)
                {
                    throw new Exception("Number of seats must be greater than 0.");
                }
                if (rentVehicleAddDTO.Seats > 45)
                {
                    throw new Exception("Number of seats cannot exceed 45.");
                }
                existingRequestDetail.StartLocation = rentVehicleAddDTO?.StartLocation;
                existingRequestDetail.EndLocation = rentVehicleAddDTO?.EndLocation;
                existingRequestDetail.StartTime = rentVehicleAddDTO?.StartTime;
                existingRequestDetail.EndTime = rentVehicleAddDTO?.EndTime;
                existingRequestDetail.Seats = rentVehicleAddDTO?.Seats;
                existingRequestDetail.UpdateAt = DateTime.Now;
                existingRequestDetail.UpdateBy = userId;

                _context.RequestDetails.Update(existingRequestDetail);


                await _context.SaveChangesAsync();


                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error in UpdateRequestRentCarAsync: {ex.Message}");
            }
        }

        public async Task<bool> CreateRequestRentCarAsync(RequestDTOForRentCar rentVehicleAddDTO)
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
                    TypeId = 2,
                    Status = false,
                    Description = "Yêu cầu thuê xe du lịch",
                    CreatedAt = DateTime.Now,
                    Note = "Chờ xác nhận",
                    CreatedBy = userId,
                    UpdateAt = DateTime.Now,
                    UpdateBy = userId,
                };


                await _context.Requests.AddAsync(addRentVehicle);
                await _context.SaveChangesAsync();

                if (string.IsNullOrWhiteSpace(rentVehicleAddDTO.StartLocation))
                {
                    throw new Exception("Start location is required.");
                }

                if (string.IsNullOrWhiteSpace(rentVehicleAddDTO.EndLocation))
                {
                    throw new Exception("End location is required.");
                }

                if (rentVehicleAddDTO.StartTime >= rentVehicleAddDTO.EndTime)
                {
                    throw new Exception("Start time must be earlier than end time.");
                }

                if (rentVehicleAddDTO.Seats <= 0)
                {
                    throw new Exception("Number of seats must be greater than 0.");
                }
                var addRentVehicleRequestDetails = new RequestDetail
                {
                    RequestId = addRentVehicle.Id,
                    VehicleId = null,
                    TicketId = null,
                    StartLocation = rentVehicleAddDTO?.StartLocation,
                    EndLocation = rentVehicleAddDTO?.EndLocation,
                    StartTime = rentVehicleAddDTO?.StartTime,
                    EndTime = rentVehicleAddDTO?.EndTime,
                    Seats = rentVehicleAddDTO?.Seats,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    UpdateAt = DateTime.Now,
                    UpdateBy = userId,
                };

                // Lưu RequestDetail vào cơ sở dữ liệu
                await _context.RequestDetails.AddAsync(addRentVehicleRequestDetails);
                await _context.SaveChangesAsync();

                // Cam kết giao dịch
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error in CreateRequestRentVehicleAsync: {ex.Message}");
            }
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
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int userId = _tokenHelper.GetIdInHeader(token);
                if (requestDTO == null)
                {
                    throw new Exception("Request data is required.");
                }

                if (string.IsNullOrWhiteSpace(requestDTO.Description))
                {
                    throw new Exception("Description is required.");
                }

                if (requestDTO.TypeId < 0)
                {
                    throw new Exception("Invalid Type ID.");
                }
                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                var newRequest = new Request
                {
                    CreatedAt = DateTime.Now,
                    Description = requestDTO.Description,
                    Note = requestDTO.Note,
                    Status = requestDTO.Status,
                    TypeId = requestDTO.TypeId,
                    UserId = userId,
                };

                await _context.Requests.AddAsync(newRequest);
                await _context.SaveChangesAsync();

                return newRequest;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateRequestVehicleAsync(int requestId, Request request)
        {
           
            try
            {
                // Kiểm tra đầu vào
                if (request == null)
                {
                    throw new Exception("Request data is required.");
                }

                if (requestId <= 0)
                {
                    throw new Exception("Invalid request ID.");
                }

                var existingRequest = await _context.Requests.SingleOrDefaultAsync(s => s.Id == requestId);
                if (existingRequest == null)
                {
                    return false;
                    throw new Exception($"Request with ID {requestId} not found.");
                }

                existingRequest.Status = request.Status;
                existingRequest.Note = request.Note;
                existingRequest.UpdateAt = DateTime.UtcNow; 
             
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the request: " + ex.Message);
            }
        }

        public async Task createRequestCancleTicket(RequestCancleTicketDTOs requestCancleTicketDTOs, int userId)
        {
            try
            {
                if (requestCancleTicketDTOs == null)
                {
                    throw new Exception("Request data is required.");
                }
                if (userId < 0)
                {
                    throw new Exception("Invalid user ID.");
                }

                DateTime dateTimeCancle = DateTime.Now.AddHours(-2);
                var listTicketId = await _context.Tickets.Where(x => x.UserId == userId && x.TimeFrom <= dateTimeCancle).ToListAsync();
                if (!listTicketId.Any())
                {
                    throw new Exception("Không có vé của nào của user");
                }
                var ticketToCancel = listTicketId.FirstOrDefault(ticket => ticket.Id == requestCancleTicketDTOs.TicketId);
                if (ticketToCancel == null)
                {
                    throw new Exception("Không có vé hợp lệ để hủy");
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
                    throw new Exception();
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
                    throw new Exception();
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
                if (rentVehicleAddDTO == null)
                {
                    throw new Exception("Request data is required.");
                }

                if (string.IsNullOrWhiteSpace(rentVehicleAddDTO.StartLocation))
                {
                    throw new Exception("Start location is required.");
                }

                if (string.IsNullOrWhiteSpace(rentVehicleAddDTO.EndLocation))
                {
                    throw new Exception("End location is required.");
                }

                if (rentVehicleAddDTO.StartTime >= rentVehicleAddDTO.EndTime)
                {
                    throw new Exception("Start time must be earlier than end time.");
                }

                if (rentVehicleAddDTO.Seats <= 0)
                {
                    throw new Exception("Number of seats must be greater than 0.");
                }

                if (rentVehicleAddDTO.Price <= 0)
                {
                    throw new Exception("Price must be greater than 0.");
                }

                var addRentVehicle = new Request
                {
                    UserId = userId,
                    TypeId = 7,
                    Status = false,
                    Description = "Tài xế muốn thuê xe của hệ thống",
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
                    StartLocation = rentVehicleAddDTO?.StartLocation,
                    EndLocation = rentVehicleAddDTO?.EndLocation,
                    StartTime = rentVehicleAddDTO?.StartTime,
                    EndTime = rentVehicleAddDTO?.EndTime,
                    Seats = rentVehicleAddDTO?.Seats,
                    Price = rentVehicleAddDTO?.Price,
                    DriverId = userId,
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
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                int vehicleOwnerId = _tokenHelper.GetIdInHeader(token);

                if (vehicleOwnerId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }
                if (await checkVehicleOwner(vehicleOwnerId, rentDriverAddDTO.VehicleId))
                {

                    var addRentDriver = new Request
                    {
                        UserId = vehicleOwnerId,
                        TypeId = 5,
                        Status = false,
                        Description = "Yêu cầu thuê tài xế",
                        CreatedAt = DateTime.Now,
                        Note = "Chờ xác nhận",
                        CreatedBy = vehicleOwnerId,
                        UpdateAt = DateTime.Now,
                        UpdateBy = vehicleOwnerId,
                    };

                    await _context.Requests.AddAsync(addRentDriver);
                    await _context.SaveChangesAsync();
                    if (rentDriverAddDTO == null)
                    {
                        throw new Exception("Request data is required.");
                    }

                    if (string.IsNullOrWhiteSpace(rentDriverAddDTO.StartLocation))
                    {
                        throw new Exception("Start location is required.");
                    }

                    if (string.IsNullOrWhiteSpace(rentDriverAddDTO.EndLocation))
                    {
                        throw new Exception("End location is required.");
                    }

                    if (rentDriverAddDTO.StartTime >= rentDriverAddDTO.EndTime)
                    {
                        throw new Exception("Start time must be earlier than end time.");
                    }

                    if (rentDriverAddDTO.Seats <= 0)
                    {
                        throw new Exception("Number of seats must be greater than 0.");
                    }

                    if (rentDriverAddDTO.Price <= 0)
                    {
                        throw new Exception("Price must be greater than 0.");
                    }

                    var addRentDriverRequestDetails = new RequestDetail
                    {
                        RequestId = addRentDriver.Id,
                        VehicleId = rentDriverAddDTO.VehicleId,
                        TicketId = null,
                        StartLocation = rentDriverAddDTO?.StartLocation,
                        EndLocation = rentDriverAddDTO?.EndLocation,
                        StartTime = rentDriverAddDTO?.StartTime,
                        EndTime = rentDriverAddDTO?.EndTime,
                        Seats = rentDriverAddDTO?.Seats,
                        Price = rentDriverAddDTO?.Price,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vehicleOwnerId,
                        UpdateAt = DateTime.Now,
                        UpdateBy = vehicleOwnerId,
                    };

                    await _context.RequestDetails.AddAsync(addRentDriverRequestDetails);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;

                }
                else
                {
                    throw new Exception("Not found vehicle or vehicle have driver");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error in CreateRequestRentDriverAsync: {ex.Message}");
            }
        }

        private async Task<bool> checkVehicleOwner(int vehicleOwnerId, int? vehicleId)
        {
            try
            {
                var vehicleOfOwner = await (from v in _context.Vehicles
                                            where v.VehicleOwner == vehicleOwnerId
                                            && v.DriverId == null
                                            select v).ToListAsync();
                foreach (var vehicle in vehicleOfOwner)
                {
                    if (vehicle.Id == vehicleId)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                PromotionCode = convenientTripDTO.PromotionCode,
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

            if (!choose)
            {
                await _context.SaveChangesAsync();
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

            if (checkRequest.TypeId != 5 && checkRequest.TypeId != 6)
            {
                throw new Exception("Invalid TypeId for determining TypeOfTrip.");
            }

            int typeOfTrip = checkRequest.TypeId == 5 ? 2 : 4;

            decimal price = checkRequestDetail.Price ?? throw new Exception("Price is required.");
            decimal? pricePromotion = null;

            if (!string.IsNullOrEmpty(checkRequestDetail.PromotionCode))
            {
                var discount = await _context.Promotions
                    .Where(p => p.CodePromotion == checkRequestDetail.PromotionCode)
                    .Select(p => p.Discount)
                    .FirstOrDefaultAsync();

                if (discount == null || discount == 0)
                {
                    throw new Exception("Invalid promotion code or no discount available.");
                }

                pricePromotion = price;
                price = price / (1 - (discount / 100m));
            }

            if (checkRequestDetail.VehicleId == 0 || checkRequestDetail.VehicleId == null)
            {
                throw new Exception("Vehicle Id null please update VehicleId before accpet!!");
            }

            var addTicket = new Ticket
            {
                Price = price,
                CodePromotion = checkRequestDetail.PromotionCode,
                PricePromotion = pricePromotion,
                NumberTicket = 2,
                PointEnd = checkRequestDetail.EndLocation,
                PointStart = checkRequestDetail.StartLocation,
                TimeFrom = checkRequestDetail.StartTime,
                TimeTo = DateTime.Now,
                UserId = checkRequest.UserId,
                VehicleId = checkRequestDetail.VehicleId,
                TripId = tripId,
                TypeOfTicket = Constant.VE_XE_TIEN_CHUYEN,
                TypeOfPayment = 2,
                Status = "Thanh toán bằng tiền mặt",
                CreatedAt = DateTime.Now,
                CreatedBy = userId,
                UpdateAt = DateTime.Now,
                UpdateBy = userId,
            };

            await _context.Tickets.AddAsync(addTicket);

            if (!string.IsNullOrEmpty(checkRequestDetail.PromotionCode))
            {
                var promotionUserId = await _context.Promotions
                    .Where(p => p.CodePromotion == checkRequestDetail.PromotionCode)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                await _promotionUserRepository.DeletePromotionAfterPayment(checkRequest.UserId, promotionUserId);
            }
            await _context.SaveChangesAsync();

            var paymentDTO = new PaymentAddDTO
            {
                UserId = addTicket.UserId,
                Code = addTicket.CodePromotion,
                Description = "",
                Price = addTicket.PricePromotion,
                TicketId = addTicket.Id,
                TypeOfPayment = 2,
                Time = DateTime.Now,
            };
            var paymentResult = await _paymentRepository.addPayment(paymentDTO);
            if (paymentResult == null)
            {
                throw new Exception("Add Payment Fails!!");
            }
            return true;
        }

        public async Task<List<RequestDTO>> getListRequestForUser(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new Exception("Invalid user ID.");
                }
                var request = await _context.Requests.Where(x => x.UserId == userId && x.TypeId != 7).ToListAsync();
                var requestMapper = _mapper.Map<List<RequestDTO>>(request);
                return requestMapper;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<RequestDTO>> GetListRequestForDriver(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new Exception("Invalid user ID.");
                }
                var request = await _context.Requests.Where(x => x.UserId == userId && x.TypeId == 7).ToListAsync();
                var requestMapper = _mapper.Map<List<RequestDTO>>(request);
                return requestMapper;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task updateRequest(int requestID, RequestDetailDTO requestDetailDTO)
        {
            try
            {
                var requestDetails = await _context.RequestDetails.FirstOrDefaultAsync(x => x.RequestId == requestID); 
                if (requestDetails == null)
                {
                    throw new Exception($"RequestDetail with Request ID {requestID} not found.");
                }
                if (string.IsNullOrWhiteSpace(requestDetailDTO.StartLocation))
                {
                    throw new Exception("Start location is required.");
                }

                if (string.IsNullOrWhiteSpace(requestDetailDTO.EndLocation))
                {
                    throw new Exception("End location is required.");
                }

                if (requestDetailDTO.StartTime >= requestDetailDTO.EndTime)
                {
                    throw new Exception("Start time must be earlier than end time.");
                }

                if (requestDetailDTO.Price <= 0)
                {
                    throw new Exception("Price must be greater than 0.");
                }
                requestDetails.StartLocation = requestDetailDTO.StartLocation;
                requestDetails.EndLocation = requestDetailDTO.EndLocation;
                requestDetails.Price = requestDetailDTO.Price;
                requestDetails.VehicleId = requestDetails.VehicleId;
                requestDetails.EndTime = requestDetailDTO.EndTime;
                requestDetails.StartTime = requestDetailDTO.StartTime;
                _context.RequestDetails.Update(requestDetails);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

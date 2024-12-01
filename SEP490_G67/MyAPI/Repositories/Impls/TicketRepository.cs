using AutoMapper;
using AutoMapper.Configuration.Conventions;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ParseStringToDateTime _parseToDateTime;
        private readonly IMapper _mapper;
        private readonly SendMail _sendMail;


        public TicketRepository(SEP490_G67Context _context, IHttpContextAccessor httpContextAccessor, ParseStringToDateTime parseToDateTime, IMapper mapper, SendMail sendMail) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _parseToDateTime = parseToDateTime;
            _mapper = mapper;
            _sendMail = sendMail;
        }

        public async Task<int> CreateTicketByUser(string? promotionCode, int tripDetailsId, BookTicketDTOs ticketDTOs, int userId, int numberTicket)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }
                if (tripDetailsId <= 0)
                {
                    throw new Exception("Invalid trip details ID.");
                }
                if (numberTicket <= 0)
                {
                    throw new Exception("Number of tickets must be greater than 0.");
                }
                var promotionUser = await (from u in _context.Users
                                           join pu in _context.PromotionUsers on u.Id equals pu.UserId
                                           join p in _context.Promotions on pu.PromotionId equals p.Id
                                           where u.Id == userId && p.CodePromotion.Equals(promotionCode)
                                           select p).FirstOrDefaultAsync();

                var promotionUserUsed = await _context.PromotionUsers.Include(x => x.Promotion)
                                        .FirstOrDefaultAsync(x => x.Promotion.CodePromotion == promotionCode && x.UserId == userId);
                //var dateString = _httpContextAccessor?.HttpContext?.Session.GetString("date") ?? DateTime.Now.ToString("MM/dd/yyyy");
                var dateString = _httpContextAccessor?.HttpContext?.Request.Cookies["date"]
                 ?? DateTime.Now.ToString("MM/dd/yyyy");

                var tripDetails = await (from td in _context.TripDetails
                                         join t in _context.Trips on td.TripId equals t.Id
                                         join vt in _context.VehicleTrips on t.Id equals vt.TripId
                                         join v in _context.Vehicles on vt.VehicleId equals v.Id
                                         where td.Id == tripDetailsId
                                         select new
                                         {
                                             TripDetails = td,
                                             Trip = t,
                                             VehicleTrip = vt,
                                             Vehicle = v
                                         }).FirstOrDefaultAsync();
                if (tripDetails == null)
                {
                    throw new Exception("Trip details not found.");
                }
                var tripbyTripDetailsId = await _context.TripDetails
                                        .Include(x => x.Trip)
                                        .Where(x => x.Id == tripDetailsId)
                                        .FirstOrDefaultAsync();
                var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripbyTripDetailsId.TripId);

                if (ticketDTOs.TypeOfPayment != Constant.CHUYEN_KHOAN && ticketDTOs.TypeOfPayment != Constant.TIEN_MAT)
                {
                    throw new Exception("Invalid type of payment.");
                }
                var createTicket = new TicketDTOs
                {
                    TripId = tripDetails.Trip.Id,
                    CodePromotion = promotionUser?.Description,
                    Description = tripDetails.Trip.Description,
                    PointStart = tripDetails.TripDetails.PointStartDetails,
                    PointEnd = tripDetails.TripDetails.PointEndDetails,
                    TimeFrom = _parseToDateTime.ParseToDateTime(dateString, trip.StartTime),
                    TimeTo = _parseToDateTime.ParseToDateTime(dateString, tripDetails.TripDetails.TimeEndDetails),
                    Price = tripDetails.Trip.Price * numberTicket,
                    PricePromotion = (tripDetails.Trip.Price * numberTicket) - (tripDetails.Trip.Price * numberTicket * (promotionUser?.Discount ?? 0) / 100),
                    NumberTicket = numberTicket,
                    TypeOfPayment = ticketDTOs.TypeOfPayment,
                    Status = (ticketDTOs.TypeOfPayment == Constant.CHUYEN_KHOAN) ? "Chờ thanh toán" : "Thanh toán bằng tiền mặt",
                    VehicleId = tripDetails.Vehicle.Id,
                    TypeOfTicket = (tripDetails.Vehicle.NumberSeat >= Constant.SO_GHE_XE_TIEN_CHUYEN) ? Constant.VE_XE_LIEN_TINH : Constant.VE_XE_TIEN_CHUYEN,
                    Note = ticketDTOs.Note + "\n" +
                        " Xe sẽ đến điểm " + tripDetails.TripDetails.PointStartDetails +
                        " vào lúc: " + tripDetails.TripDetails.PointStartDetails,
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    UpdateAt = null,
                    UpdateBy = null
                };
                var createTicketMapper = _mapper.Map<Ticket>(createTicket);
                _context.Tickets.Add(createTicketMapper);
                await _context.SaveChangesAsync();
                var ticketId = createTicketMapper.Id;
                var promotionUserMapper = _mapper.Map<PromotionUser>(promotionUserUsed);
                if (promotionUser != null) _context.PromotionUsers.Remove(promotionUserMapper);
                await _context.SaveChangesAsync();
                var pointUser = new PointUser
                {
                    Points = (int?)((int?) createTicket.PricePromotion * Constant.TICH_DIEM),
                    UserId = userId,
                    PointsMinus = 0,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    UpdateAt = DateTime.Now,
                    UpdateBy = userId
                };
                _context.PointUsers.Add(pointUser);
                await _context.SaveChangesAsync();
                return ticketId;
            }
            catch (Exception ex)
            {
                throw new Exception("CreateTicketByUser: " + ex.StackTrace);
            }
        }

        public async Task CreatTicketFromDriver(int priceTrip, int vehicleId, TicketFromDriverDTOs ticket, int driverId, int numberTicket)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }

                if (driverId <= 0)
                {
                    throw new Exception("Invalid driver ID.");
                }
                if (numberTicket <= 0)
                {
                    throw new Exception("Number of tickets must be greater than 0.");
                }
                if (ticket == null)
                {
                    throw new Exception("Ticket information is required.");
                }
                if (string.IsNullOrWhiteSpace(ticket.PointStart))
                {
                    throw new Exception("PointStart is required.");
                }

                if (string.IsNullOrWhiteSpace(ticket.PointEnd))
                {
                    throw new Exception("PointEnd is required.");
                }

                if (ticket.TypeOfPayment != Constant.CHUYEN_KHOAN && ticket.TypeOfPayment != Constant.TIEN_MAT)
                {
                    throw new Exception("Invalid type of payment.");
                }
                if (ticket.PointStart != null && ticket.PointEnd != null)

                {
                    TicketDTOs createTicketFromDriver = new TicketDTOs
                    {
                        Price = priceTrip * numberTicket,
                        PricePromotion = priceTrip * numberTicket,
                        PointStart = ticket.PointStart,
                        PointEnd = ticket.PointEnd,
                        TimeFrom = DateTime.Now,
                        NumberTicket = numberTicket,
                        Description = "Khách bắt dọc đường di chuyển từ " + ticket.PointStart + " đến " + ticket.PointEnd,
                        VehicleId = vehicleId,
                        TypeOfPayment = ticket.TypeOfPayment,
                        Status = "Đã thanh toán",
                        CreatedAt = DateTime.Now,
                        CreatedBy = driverId,
                    };
                    var tickerMapper = _mapper.Map<Ticket>(createTicketFromDriver);
                    _context.Tickets.Add(tickerMapper);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //note
        public async Task AcceptOrDenyRequestRentCar(int requestId, bool choose, int vehicleId, decimal price)
        {
            try
            {
                var request = await _context.Requests
                    .Include(r => r.RequestDetails)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                {
                    throw new Exception("Request not found");
                }

                if (!choose)
                {
                    request.Status = false;
                    await _context.SaveChangesAsync();
                    return;
                }

                var requestDetail = request.RequestDetails.FirstOrDefault();
                if (requestDetail == null)
                {
                    throw new Exception("Request details not found");
                }

                var ticket = new Ticket
                {
                    VehicleId = vehicleId,
                    Price = price,
                    NumberTicket = requestDetail.Seats,
                    PointStart = requestDetail.StartLocation,
                    PointEnd = requestDetail.EndLocation,
                    TimeFrom = requestDetail.StartTime,
                    TimeTo = requestDetail.EndTime,
                    Description = request.Description,
                    Note = request.Note,
                    UserId = request.UserId,
                    TypeOfTicket = Constant.VE_XE_DU_LICH,
                    TypeOfPayment = Constant.CHUYEN_KHOAN,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.UserId,
                    UpdateAt = DateTime.UtcNow,
                    UpdateBy = Constant.ADMIN,
                    Status = "Approved"
                };

                await _context.Tickets.AddAsync(ticket);

                request.Status = true;
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(request.UserId);
                if (user != null)
                {
                    SendMailDTO sendMailDTO = new()
                    {
                        FromEmail = "duclinh5122002@gmail.com",
                        Password = "jetj haze ijdw euci",
                        ToEmail = user.Email,
                        Subject = "Request Confirmation",
                        Body = $"Your ticket request from {ticket.PointStart} to {ticket.PointEnd} has been approved. Thank you for your request!"
                    };

                    if (!await _sendMail.SendEmail(sendMailDTO))
                    {
                        throw new Exception("Failed to send confirmation email.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("AcceptOrDenyRequestRentCar: " + ex.Message);
            }
        }

        public async Task<bool> UpdateVehicleInRequestAsync(int vehicleId, int requestId)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }
                var requestDetail = await _context.RequestDetails.FirstOrDefaultAsync(rd => rd.RequestId == requestId);

                if (requestDetail == null)
                {
                    return false;
                }


                requestDetail.VehicleId = vehicleId;


                _context.RequestDetails.Update(requestDetail);
                await _context.SaveChangesAsync();

                return true;
            }
            catch(Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }
        //note
        public async Task<IEnumerable<VehicleBasicDto>> GetVehiclesByRequestIdAsync(int requestId)
        {
            try
            {
                var requestDetail = await _context.RequestDetails
                    .FirstOrDefaultAsync(rd => rd.RequestId == requestId);

                if (requestDetail == null || requestDetail.Seats == null)
                {
                    return Enumerable.Empty<VehicleBasicDto>();
                }

                var seatCount = requestDetail.Seats.Value;

                var vehicles = await _context.Vehicles
                    .Where(v => v.VehicleTypeId == 3 && v.NumberSeat >= seatCount)
                    .Take(5)
                    .Select(v => new VehicleBasicDto
                    {
                        Id = v.Id,
                        NumberSeat = v.NumberSeat,
                        VehicleTypeId = v.VehicleTypeId,
                        Status = v.Status,
                        LicensePlate = v.LicensePlate,
                        Description = v.Description
                    })
                    .ToListAsync();

                return vehicles;
            }
            catch (Exception)
            {
                return Enumerable.Empty<VehicleBasicDto>();
            }
        }


        public async Task<List<ListTicketDTOs>> getAllTicket()
        {
            try
            {
                var listTicket = await _context.Tickets.ToListAsync();
                var listTicketMapper = _mapper.Map<List<ListTicketDTOs>>(listTicket);
                return listTicketMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("getAllTicket: " + ex.Message);
            }
        }

        public async Task<TicketNotPaidSummary> GetListTicketNotPaid(int vehicleId)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }
                var listTicket = await (from v in _context.Vehicles
                                        join vt in _context.VehicleTrips
                                        on v.Id equals vt.VehicleId
                                        join t in _context.Trips
                                        on vt.TripId equals t.Id
                                        join tk in _context.Tickets
                                        on t.Id equals tk.TripId
                                        join u in _context.Users on tk.UserId equals u.Id
                                        join typeP in _context.TypeOfPayments on tk.TypeOfPayment equals typeP.Id
                                        where typeP.Id == Constant.TIEN_MAT &&
                                        v.Id == vehicleId &&
                                        tk.Status == "Thanh toán bằng tiền mặt" &&
                                        tk.TimeFrom <= DateTime.Now
                                        select new { tk.UserId, u.FullName, tk.PricePromotion, typeP.TypeOfPayment1, tk.Id }
                                       ).ToListAsync();
                var totalPricePromotion = listTicket
                    .GroupBy(t => new { t.UserId, t.Id })
                    .Select(g => new TicketNotPaid { ticketId = g.Key.Id, userId = g.Key.UserId, FullName = g.FirstOrDefault()?.FullName, Price = g.Sum(x => x.PricePromotion.Value), TypeOfPayment = g.FirstOrDefault()?.TypeOfPayment1 })
                    .ToList();
                decimal total = totalPricePromotion.Sum(t => t.Price);
                var result = new TicketNotPaidSummary
                {
                    Tickets = totalPricePromotion,
                    Total = total

                };
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("GetListTicketNotPaid: " + ex.Message);
            }
        }

        public async Task<int> GetPriceFromPoint(TicketFromDriverDTOs ticket, int vehicleId)
        {
            try
            {
                if (ticket == null)
                {
                    throw new Exception("Ticket data is required.");
                }
                if (vehicleId <= 0)
                {
                    throw new Exception("Invalid vehicle ID.");
                }
                if (string.IsNullOrWhiteSpace(ticket.PointStart))
                {
                    throw new Exception("PointStart is required.");
                }

                if (string.IsNullOrWhiteSpace(ticket.PointEnd))
                {
                    throw new Exception("PointEnd is required.");
                }

                var priceFromPoint = await (from v in _context.Vehicles
                                            join vt in _context.VehicleTrips
                                            on v.Id equals vt.VehicleId
                                            join t in _context.Trips
                                            on vt.TripId equals t.Id
                                            where t.PointStart.Equals(ticket.PointStart) && t.PointEnd.Equals(ticket.PointEnd)
                                            select t.Price
                                            ).FirstOrDefaultAsync();
                if (priceFromPoint == null)
                {
                    throw new ArgumentNullException(nameof(priceFromPoint), "Price not found for the specified route.");
                }
                return Convert.ToInt32(priceFromPoint.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("GetPriceFromPoint: " + ex.Message);
            }
        }

        public async Task UpdateStatusTicketNotPaid(int id, int driverId)
        {
            try
            {
                if (id <= 0)
                {
                    throw new Exception("Invalid ticket ID.");
                }

                if (driverId <= 0)
                {
                    throw new Exception("Invalid driver ID.");
                }
                var ticketNotPaid = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id && x.TypeOfPayment == Constant.TIEN_MAT);

                var pointTicket = ticketNotPaid.PricePromotion * 10 / 100;

                var paymentTicket = new Payment
                {
                    UserId = ticketNotPaid.UserId,
                    Code = "NULL",
                    Description = "THANH TOÁN TIỀN MẶT",
                    Price = ticketNotPaid.Price,
                    Time = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedBy = driverId,
                    TypeOfPayment = Constant.TIEN_MAT,
                };
                await _context.Payments.AddAsync(paymentTicket);
                await _context.SaveChangesAsync();

                var addPointUser = new PointUser
                {
                    Points = (int)pointTicket,
                    UserId = ticketNotPaid.UserId,
                    Date = DateTime.Now,
                    PaymentId = paymentTicket.PaymentId,
                    CreatedBy = driverId,
                    CreatedAt = DateTime.Now,
                    PointsMinus = 0
                };
                await _context.PointUsers.AddAsync(addPointUser);
                await _context.SaveChangesAsync();
                if (ticketNotPaid != null)
                {
                    ticketNotPaid.Status = "Đã thanh toán";
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateStatusTicketNotPaid: " + ex.Message);
            }
        }

        public async Task<TicketByIdDTOs> getTicketById(int ticketId)
        {
            try
            {
                if (ticketId <= 0)
                {
                    throw new Exception("Invalid ticket ID.");
                }
                var ticketById = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
                var mapperTicketById = _mapper.Map<TicketByIdDTOs>(ticketById);
                return mapperTicketById;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateStatusTicketForPayment(int id)
        {
            try
            {

                var checkTicket = await _context.Tickets.FirstOrDefaultAsync(s => s.Id == id);
                if (checkTicket != null)
                {
                    checkTicket.Status = "Đã thanh toán bằng tài khoản";
                    checkTicket.UpdateAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    throw new Exception("Not found ticket");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<RevenueTicketDTO> getRevenueTicket(DateTime startTime, DateTime endTime, int? vehicleId, int? vehicleOwner, int userId)
        {
            try
            {
                if (startTime > endTime)
                {
                    throw new Exception("Start time must be earlier than or equal to end time.");
                }

                if (userId <= 0)
                {
                    throw new Exception("Invalid user ID.");
                }
                if (startTime == default || endTime == default)
                {
                    throw new Exception("Start time and end time must be valid DateTime values.");
                }
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (getInforUser == null)
                {
                    throw new Exception("User not found.");
                }
                if (IsUserRole(getInforUser, "VehicleOwner"))
                {
                    return await GetRevenueForVehicleOwner(startTime, endTime, vehicleId, userId);
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetRevenueForStaff(startTime, endTime, vehicleId, vehicleOwner, userId);
                }
                throw new Exception("User role is not supported.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool IsUserRole(User user, string roleName)
        {
            return user.UserRoles.Any(ur => ur.Role.RoleName == roleName);
        }
        private async Task<RevenueTicketDTO> GetRevenueForVehicleOwner(DateTime startTime, DateTime endTime, int? vehicleId, int userId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).Where(x => x.Vehicle.VehicleOwner == userId && x.CreatedAt >= startTime && x.CreatedAt <= endTime);
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            return await GetRevenueTicketDTO(query);
        }
        private async Task<RevenueTicketDTO> GetRevenueForStaff(DateTime startTime, DateTime endTime, int? vehicleId, int? vehicleOwner, int userId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).Where(x => x.CreatedAt >= startTime && x.CreatedAt <= endTime);
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            if (vehicleOwner.HasValue && vehicleOwner != 0)
            {
                query = query.Where(x => x.Vehicle.VehicleOwner == vehicleOwner);
            }
            if (!vehicleId.HasValue && vehicleId == 0 && !vehicleOwner.HasValue && vehicleOwner == 0)
            {
                query = query;
            }
            return await GetRevenueTicketDTO(query);
        }
        private async Task<RevenueTicketDTO> GetRevenueTicketDTO(IQueryable<Ticket> query)
        {
            var listTicket =
                await query.Select(
                    x => new TicketRevenue
                    {
                        PricePromotion = x.PricePromotion,
                        CreatedAt = x.CreatedAt,
                        LiscenseVehicle = x.Vehicle.LicensePlate,
                        TypeOfTicket = x.TypeOfTicketNavigation.Description,
                        TypeOfPayment = x.TypeOfPaymentNavigation.TypeOfPayment1,
                    }).ToListAsync();
            var sumPriceTicket = query.Sum(x => x.PricePromotion);
            var combineResult = new RevenueTicketDTO
            {
                total = sumPriceTicket,
                listTicket = listTicket
            };
            return combineResult;
        }

        public async Task<bool> deleteTicketTimeOut(int ticketId)
        {
            var checkTicket = await _context.Tickets.FirstOrDefaultAsync(s => s.Id == ticketId);

            if (checkTicket == null)
            {
                throw new Exception("Ticket id not found");
            }

            _context.Tickets.Remove(checkTicket);
            await _context.SaveChangesAsync();
            return true;
        }
        [Authorize]
        public async Task<List<ListTicketDTOs>> GetTicketByUserId(int userId)
        {
            try
            {
              
                if (userId <= 0)
                {
                    throw new Exception("Invalid user.");
                }
                var listTicket = await _context.Tickets.Where(x => x.UserId == userId).ToListAsync();
                var mapper = _mapper.Map<List<ListTicketDTOs>>(listTicket);
                return mapper;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

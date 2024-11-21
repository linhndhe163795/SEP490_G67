using AutoMapper;
using AutoMapper.Configuration.Conventions;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.TicketDTOs;
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
                    throw new NullReferenceException();
                }
                var promotionUser = await (from u in _context.Users
                                           join pu in _context.PromotionUsers on u.Id equals pu.UserId
                                           join p in _context.Promotions on pu.PromotionId equals p.Id
                                           where u.Id == userId && p.CodePromotion.Equals(promotionCode)
                                           select p).FirstOrDefaultAsync();

                var promotionUserUsed = await _context.PromotionUsers.Include(x => x.Promotion)
                                        .FirstOrDefaultAsync(x => x.Promotion.CodePromotion == promotionCode && x.UserId == userId);
                var dateString = _httpContextAccessor?.HttpContext?.Session.GetString("date") ?? DateTime.Now.ToString("MM/dd/yyyy"); ;

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

                var createTicket = new TicketDTOs
                {
                    TripId = tripDetails.Trip.Id,
                    CodePromotion = promotionUser?.Description,
                    Description = tripDetails.Trip.Description,
                    PointStart = tripDetails.TripDetails.PointStartDetails,
                    PointEnd = tripDetails.TripDetails.PointEndDetails,
                    TimeFrom = _parseToDateTime.ParseToDateTime(dateString, tripDetails.TripDetails.TimeStartDetils),
                    TimeTo = _parseToDateTime.ParseToDateTime(dateString, tripDetails.TripDetails.TimeEndDetails),
                    Price = tripDetails.Trip.Price * numberTicket,
                    PricePromotion = (tripDetails.Trip.Price * numberTicket) - (tripDetails.Trip.Price * numberTicket * (promotionUser?.Discount ?? 0) / 100),
                    NumberTicket = numberTicket,
                    TypeOfPayment = ticketDTOs.TypeOfPayment,
                    Status = (ticketDTOs.TypeOfPayment == Constant.CHUYEN_KHOAN) ? "Chờ thanh toán" : "Thanh toán bằng tiền mặt",
                    VehicleId = tripDetails.Vehicle.Id,
                    TypeOfTicket = (tripDetails.Vehicle.NumberSeat >= Constant.SO_GHE_XE_TIEN_CHUYEN) ? Constant.VE_XE_LIEN_TINH : Constant.VE_XE_TIEN_CHUYEN,
                    Note = ticketDTOs.Note,
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
                return ticketId;
            }
            catch (Exception ex)
            {
                throw new Exception("CreateTicketByUser: " + ex.Message);
            }
        }

        public async Task CreatTicketFromDriver(int priceTrip, int vehicleId, TicketFromDriverDTOs ticket, int driverId, int numberTicket)
        {
            try
            {
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
        public async Task AcceptOrDenyRequestRentCar(int requestId, bool choose)
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
                    VehicleId = requestDetail.VehicleId,
                    Price = requestDetail.Price,
                    //CodePromotion = requestDetail.CodePromotion,
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
                    Status = "Approved"
                };

                //if (!string.IsNullOrEmpty(requestDetail.CodePromotion))
                //{
                //    var promotion = await _context.Promotions
                //        .FirstOrDefaultAsync(p => p.CodePromotion == requestDetail.CodePromotion);

                //    if (promotion != null)
                //    {
                //        ticket.PricePromotion = requestDetail.Price - (requestDetail.Price * (promotion.Discount / 100.0m));
                //    }
                //}

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

        public async Task<List<TicketNotPaid>> GetListTicketNotPaid(int vehicleId)
        {
            try
            {
                var listTicket = await (from v in _context.Vehicles
                                        join vt in _context.VehicleTrips
                                        on v.Id equals vt.VehicleId
                                        join t in _context.Trips
                                        on vt.TripId equals t.Id
                                        join tk in _context.Tickets
                                        on t.Id equals tk.TripId
                                        join u in _context.Users on tk.UserId equals u.Id
                                        join typeP in _context.TypeOfPayments on tk.TypeOfPayment equals typeP.Id
                                        where typeP.Id == Constant.TIEN_MAT && v.Id == vehicleId
                                        select new { u.FullName, tk.PricePromotion, typeP.TypeOfPayment1 }
                                       ).ToListAsync();
                var totalPricePromotion = listTicket
                    .GroupBy(t => t.FullName)
                    .Select(g => new TicketNotPaid { FullName = g.Key, Price = g.Sum(x => x.PricePromotion.Value), TypeOfPayment = g.FirstOrDefault()?.TypeOfPayment1 })
                    .ToList();


                return totalPricePromotion;
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

        public async Task UpdateStatusTicketNotPaid(int id)
        {
            try
            {
                var ticketNotPaid = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id && x.TypeOfPayment == Constant.TIEN_MAT);
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
                    throw new Exception("Update for payment");
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
            var query = _context.Tickets.Include(x => x.Vehicle).Where(x => x.Vehicle.VehicleOwner == userId);
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
                        VehicleId = x.VehicleId,
                        TypeOfTicket = x.TypeOfTicket,
                        TypeOfPayment = x.TypeOfPayment                        
                    }).ToListAsync();
            var sumPriceTicket = query.Sum(x => x.Price);
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
    }
}

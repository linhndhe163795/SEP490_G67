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
        public async Task CreateTicketForRentCar(int vehicleId, decimal price, TicketForRentCarDTO ticketRentalDTO, int userId)
        {
            try
            {
                var promotionUser = await (from u in _context.Users
                                           join pu in _context.PromotionUsers on u.Id equals pu.UserId
                                           join p in _context.Promotions on pu.PromotionId equals p.Id
                                           where u.Id == userId && p.CodePromotion.Equals(ticketRentalDTO.CodePromotion)
                                           select p).FirstOrDefaultAsync();

                var promotionUserUsed = await _context.PromotionUsers.Include(x => x.Promotion)
                                                      .FirstOrDefaultAsync(x => x.Promotion.CodePromotion == ticketRentalDTO.CodePromotion && x.UserId == userId);

                var ticket = new Ticket
                {
                    VehicleId = vehicleId,
                    Price = price,
                    CodePromotion = promotionUser?.Description,
                    NumberTicket = ticketRentalDTO.NumberTicket,
                    PointStart = ticketRentalDTO.PointStart,
                    PointEnd = ticketRentalDTO.PointEnd,
                    TimeFrom = ticketRentalDTO.TimeFrom,
                    TimeTo = ticketRentalDTO.TimeTo,
                    Description = ticketRentalDTO.Description,
                    Note = ticketRentalDTO.Note,
                    UserId = userId,
                    TypeOfTicket = Constant.VE_XE_DU_LICH,
                    TypeOfPayment = ticketRentalDTO.TypeOfPayment,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Đã thanh toán"
                };

                if (promotionUser != null)
                {
                    ticket.PricePromotion = price - (price * (promotionUser.Discount / 100.0m));
                }


                await _context.Tickets.AddAsync(ticket);


                if (promotionUserUsed != null)
                {
                    _context.PromotionUsers.Remove(promotionUserUsed);
                }


                await _context.SaveChangesAsync();


                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {

                    SendMailDTO sendMailDTO = new()
                    {
                        FromEmail = "duclinh5122002@gmail.com",
                        Password = "jetj haze ijdw euci",
                        ToEmail = user.Email,
                        Subject = "Ticket Confirmation",
                        Body = $"Your ticket from {ticket.PointStart} to {ticket.PointEnd} has been successfully created. Thank you for your purchase!"
                    };

                    if (!await _sendMail.SendEmail(sendMailDTO))
                    {
                        throw new Exception("Failed to send confirmation email.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("CreateTicketForRentCar: " + ex.Message);
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

        public async Task<RevenueTicketDTO> getRevenueTicket(int? vehicleId, int? vehicleOwner, int userId)
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
                    return await GetRevenueForVehicleOwner(vehicleId, userId);
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetRevenueForStaff(vehicleId, vehicleOwner, userId);
                }
                throw new Exception("User role is not supported.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private bool IsUserRole(User user, string roleName)
        {
            return user.UserRoles.Any(ur => ur.Role.RoleName == roleName);
        }
        private async Task<RevenueTicketDTO> GetRevenueForVehicleOwner(int? vehicleId, int userId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).Where(x => x.Vehicle.VehicleOwner == userId);
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            return await GetRevenueTicketDTO(query);
        }
        private async Task<RevenueTicketDTO> GetRevenueForStaff(int? vehicleId, int? vehicleOwner, int userId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).AsQueryable();
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            if (vehicleOwner.HasValue && vehicleOwner != 0)
            {
                query = query.Where(x => x.Vehicle.VehicleOwner == vehicleOwner);
            }
            return await GetRevenueTicketDTO(query);
        }
        private async Task<RevenueTicketDTO> GetRevenueTicketDTO(IQueryable<Ticket> query)
        {
            var listTicket =
                await query.Select(
                    x => new TicketDTOs
                    {
                        Price = x.Price,
                        CreatedAt = x.CreatedAt,
                        VehicleId = x.VehicleId,
                        TypeOfTicket = x.TypeOfTicket,
                    }).ToListAsync();
            var sumPriceTicket = query.Sum(x => x.Price);
            var combineResult = new RevenueTicketDTO
            {
                total = sumPriceTicket,
                listTicket = listTicket
            };
            return combineResult;
        }
    }
}

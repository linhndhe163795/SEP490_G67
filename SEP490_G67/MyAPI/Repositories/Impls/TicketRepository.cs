using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        public TicketRepository(SEP490_G67Context _context, IHttpContextAccessor httpContextAccessor, ParseStringToDateTime parseToDateTime, IMapper mapper) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _parseToDateTime = parseToDateTime;
            _mapper = mapper;
        }

        public async Task CreateTicketByUser(string? promotionCode, int tripDetailsId, TicketDTOs ticketDTOs, int userId)
        {
            try
            {
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
                    Price = tripDetails.Trip.Price,
                    PricePromotion = tripDetails.Trip.Price - (tripDetails.Trip.Price * (promotionUser?.Discount ?? 0) / 100),
                    SeatCode = null,
                    TypeOfPayment = ticketDTOs.TypeOfPayment,
                    Status = (ticketDTOs.TypeOfPayment == 1) ? "Đã thanh toán" : "Chưa thanh toán",
                    VehicleId = tripDetails.Vehicle.Id,
                    TypeOfTicket = (tripDetails.Vehicle.NumberSeat > 7) ? 1 : 0,
                    Note = ticketDTOs.Note,
                    UserId = ticketDTOs.UserId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = ticketDTOs.UserId,
                    UpdateAt = null,
                    UpdateBy = null
                };
                var createTicketMapper = _mapper.Map<Ticket>(createTicket);
                _context.Tickets.Add(createTicketMapper);
                var promotionUserMapper = _mapper.Map<PromotionUser>(promotionUserUsed);
                if (promotionUser != null) _context.PromotionUsers.Remove(promotionUserMapper);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("CreateTicketByUser: " + ex.Message);
            }
        }

        public async Task CreatTicketFromDriver(int priceTrip, int vehicleId, TicketFromDriverDTOs ticket, int driverId)
        {
            try
            {
                if (ticket.PointStart != null && ticket.PointEnd != null)

                {
                    TicketDTOs createTicketFromDriver = new TicketDTOs
                    {
                        Price = priceTrip,
                        PricePromotion = priceTrip,
                        PointStart = ticket.PointStart,
                        PointEnd = ticket.PointEnd,
                        TimeFrom = DateTime.Now,
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
                                       where typeP.Id == 2 && v.Id == vehicleId
                                       select new { u.FullName, tk.PricePromotion , typeP.TypeOfPayment1 }
                                       ).ToListAsync();
                var totalPricePromotion = listTicket
                    .GroupBy(t => t.FullName)
                    .Select(g => new TicketNotPaid {FullName = g.Key,Price = g.Sum(x => x.PricePromotion.Value), TypeOfPayment = g.FirstOrDefault()?.TypeOfPayment1 })
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


    }
}

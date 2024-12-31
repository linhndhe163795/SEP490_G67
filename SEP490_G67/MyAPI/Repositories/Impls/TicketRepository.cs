using AutoMapper;
using AutoMapper.Configuration.Conventions;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq;
using System.Net.Sockets;

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
        public async Task<int> CreateTicketByUser(string? promotionCode, int tripDetailsId, BookTicketDTOs ticketDTOs, int userId, int numberTicket, DateTime dateTicket)
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
                var dateString = dateTicket.ToString("MM/dd/yyyy");

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
                    Note =
                        " Xe sẽ đến điểm " + tripDetails.TripDetails.PointStartDetails +
                        " vào lúc: " + tripDetails.TripDetails.TimeStartDetils,
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
                    Points = (int?)((int?)createTicket.PricePromotion * Constant.TICH_DIEM),
                    UserId = userId,
                    PointsMinus = 0,
                    CreatedAt = DateTime.Now,
                    Date = DateTime.Now,
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
                var trip = await _context.Trips.Include(x => x.TripDetails).Where(x => x.PointStart.Equals(ticket.PointStart) && x.PointEnd.Equals(ticket.PointEnd)).FirstOrDefaultAsync();
                var timeTo = trip.TripDetails.Select(x => x.TimeEndDetails).FirstOrDefault() ?? TimeSpan.MaxValue;
                var currentDate = DateTime.Today;
                var dateTimeTo = currentDate.Add(timeTo);

                if (ticket.PointStart != null && ticket.PointEnd != null)

                {
                    TicketDTOs createTicketFromDriver = new TicketDTOs
                    {
                        Price = priceTrip * numberTicket,
                        PricePromotion = priceTrip * numberTicket,
                        PointStart = ticket.PointStart,
                        PointEnd = ticket.PointEnd,
                        TimeFrom = DateTime.Now,
                        TimeTo = dateTimeTo,
                        TypeOfTicket = Constant.VE_XE_LIEN_TINH,
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
        public async Task AcceptOrDenyRequestRentCar(AddTicketForRentCarDTO addTicketForRentCarDTO)
        {
            try
            {
                var request = await _context.Requests
                    .Include(r => r.RequestDetails)
                    .FirstOrDefaultAsync(r => r.Id == addTicketForRentCarDTO.requestId);

                if (request == null)
                {
                    throw new Exception("Request not found");
                }

                if (!addTicketForRentCarDTO.choose)
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

                var type_id = await _context.Requests.Where(s => s.Id == addTicketForRentCarDTO.requestId).Select(r => r.TypeId).FirstOrDefaultAsync();

                if (type_id != 2)
                {
                    throw new Exception("Request for type rent car!!!");
                }

                requestDetail.VehicleId = addTicketForRentCarDTO.vehicleId;
                requestDetail.Price = addTicketForRentCarDTO.price;

                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == addTicketForRentCarDTO.vehicleId);
                if (vehicle == null)
                {
                    throw new Exception("Vehicle not found");
                }

                vehicle.DateStartBusy = requestDetail.StartTime;
                vehicle.DateEndBusy = requestDetail.EndTime;

                var ticket = new Ticket
                {
                    VehicleId = addTicketForRentCarDTO.vehicleId,
                    Price = addTicketForRentCarDTO.price,
                    PricePromotion = addTicketForRentCarDTO.price,
                    NumberTicket = requestDetail.Seats,
                    PointStart = requestDetail.StartLocation,
                    PointEnd = requestDetail.EndLocation,
                    TimeFrom = requestDetail.StartTime,
                    TimeTo = requestDetail.EndTime,
                    Description = request.Description,
                    Note = "Đã xác nhận",
                    UserId = request.UserId,
                    TypeOfTicket = Constant.VE_XE_DU_LICH,
                    TypeOfPayment = Constant.TIEN_MAT,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.UserId,
                    UpdateAt = DateTime.UtcNow,
                    UpdateBy = Constant.ADMIN,
                    Status = "Thanh toán bằng tiền mặt"
                };

                await _context.Tickets.AddAsync(ticket);

                request.Status = true;
                request.Note = "Đã xác nhận";
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
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<VehicleBasicDto>> GetVehiclesByRequestIdAsync(int requestId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // xe du lịch đang rảnh
                var requestDetail = await _context.RequestDetails
                    .FirstOrDefaultAsync(rd => rd.RequestId == requestId);

                if (requestDetail == null || requestDetail.Seats == null)
                {
                    return Enumerable.Empty<VehicleBasicDto>();
                }

                var seatCount = requestDetail.Seats.Value;
                var ticket = await _context.Tickets.Where(t => t.TimeFrom < endDate &&
                                        t.TimeTo > startDate && t.TypeOfTicket == Constant.VE_XE_DU_LICH).ToListAsync();
                var ticketVehicleIds = ticket
                                        .Where(t => t.VehicleId.HasValue)
                                        .Select(t => t.VehicleId.Value)
                                        .ToHashSet();

                var vehicle = await (from v in _context.Vehicles
                                     join d in _context.Drivers
                                     on v.DriverId equals d.Id into driverGroup
                                     from d in driverGroup.DefaultIfEmpty()
                                     join u in _context.Users
                                     on v.VehicleOwner equals u.Id into userGroup
                                     from u in userGroup.DefaultIfEmpty()
                                     join t in _context.Tickets
                                     on v.Id equals t.VehicleId into ticketGroup
                                     from t in ticketGroup.DefaultIfEmpty()
                                     where v.VehicleTypeId == Constant.VE_XE_DU_LICH && !ticketVehicleIds.Contains(v.Id)
                                     select new VehicleBasicDto
                                     {
                                         Id = v.Id,
                                         NumberSeat = v.NumberSeat,
                                         VehicleTypeId = v.VehicleTypeId,
                                         Status = v.Status,
                                         DriverId = d.Id != 0 ? d.Id : 0,
                                         VehicleOwner = u.Id,
                                         VehicleOwnerName = $"{u.Id} - Tên chủ xe: {u.FullName} - SĐT: {u.NumberPhone}",
                                         LicensePlate = $"{v.LicensePlate} - Số ghế: {v.NumberSeat}- Tên chủ xe: {u.FullName} - SĐT: {u.NumberPhone}",
                                         DriverName = $"{(d.Name != null ? d.Name : null)} - SĐT: {d.NumberPhone}",
                                     }).Distinct().ToListAsync();

                return vehicle;
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
                var listTicket = await _context.Tickets.OrderByDescending(x => x.Id).ToListAsync();
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
                                        select new { tk.UserId, u.FullName, tk.PricePromotion, typeP.TypeOfPayment1, tk.Id, v.LicensePlate }
                                       ).ToListAsync();
                var totalPricePromotion = listTicket
                    .GroupBy(t => new { t.UserId, t.Id, t.LicensePlate })
                    .Select(g => new TicketNotPaid
                    {
                        ticketId = g.Key.Id,
                        userId = g.Key.UserId,
                        LicensePlate = g.Key.LicensePlate,
                        FullName = g.FirstOrDefault()?.FullName,
                        Price = g.Sum(x => x.PricePromotion.Value),
                        TypeOfPayment = g.FirstOrDefault()?.TypeOfPayment1
                    })
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
                if (ticketNotPaid == null)
                {
                    throw new Exception("Not ticket valid");
                }
                var pointTicket = ticketNotPaid.PricePromotion * 10 / 100;

                var vehicleIDs = await _context.Vehicles
            .Where(x => x.DriverId == driverId)
            .Select(x => x.Id)
            .ToListAsync();

                // Kiểm tra nếu VehicleId của vé có giá trị và nằm trong danh sách các xe
                if (!ticketNotPaid.VehicleId.HasValue || !vehicleIDs.Contains(ticketNotPaid.VehicleId.Value))
                {
                    throw new Exception("Driver is not assigned to this vehicle.");
                }

                var addPointUser = new PointUser
                {
                    Points = (int)pointTicket,
                    UserId = ticketNotPaid.UserId,
                    Date = DateTime.Now,
                    PaymentId = null,
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
        public async Task<TicketByIdDTOs> getTicketDetailsById(int ticketId, int userId)
        {
            try
            {
                if (ticketId <= 0)
                {
                    throw new ArgumentException("Invalid ticket ID.");
                }

                var getInforUser = await _context.Users
                    .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(x => x.Id == userId);

                if (getInforUser == null)
                {
                    throw new ArgumentException("User not found.");
                }
                var query = (from t in _context.Tickets
                             join tp in _context.TypeOfPayments
                             on t.TypeOfPayment equals tp.Id
                             join tt in _context.TypeOfTickets
                             on t.TypeOfTicket equals tt.Id
                             join v in _context.Vehicles
                             on t.VehicleId equals v.Id
                             join u in _context.Users
                             on t.UserId equals u.Id
                             where t.Id == ticketId
                             select new TicketByIdDTOs
                             {
                                 VehicleId = v.Id,
                                 CodePromotion = t.CodePromotion,
                                 Price = t.Price,
                                 PricePromotion = t.PricePromotion,
                                 Description = t.Description,
                                 Status = t.Status,
                                 TimeFrom = t.TimeFrom,
                                 TimeTo = t.TimeTo,
                                 Note = t.Note,
                                 PointStart = t.PointStart,
                                 PointEnd = t.PointEnd,
                                 TripId = t.TripId,
                                 TypeOfPayment = tp.TypeOfPayment1,
                                 LicsenceVehicle = v.LicensePlate,
                                 UserId = t.UserId,
                                 fullName = u.FullName
                             });
                if (IsUserRole(getInforUser, "Staff"))
                {
                    var ticketById = await query.FirstOrDefaultAsync();

                    if (ticketById == null)
                    {
                        throw new ArgumentException("Ticket not found.");
                    }

                    return ticketById;
                }

                if (IsUserRole(getInforUser, "User"))
                {
                    var ticket = await query.FirstOrDefaultAsync(x => x.UserId == userId);

                    if (ticket == null)
                    {
                        throw new ArgumentException("Ticket not found or you do not have access to this ticket.");
                    }

                    return ticket;
                }

                throw new UnauthorizedAccessException("User does not have valid roles to access the ticket.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching ticket details: {ex.Message}", ex);
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
        public async Task<RevenueTicketDTO> getRevenueTicket(int userId)
        {
            try
            {

                if (userId <= 0)
                {
                    throw new Exception("Invalid user ID.");
                }
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (getInforUser == null)
                {
                    throw new Exception("User not found.");
                }
                if (IsUserRole(getInforUser, "VehicleOwner"))
                {
                    return await GetRevenueForVehicleOwner(userId);
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetRevenueForStaff(userId);
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
        private async Task<RevenueTicketDTO> GetRevenueForVehicleOwner(int userId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).Where(x => x.Vehicle.VehicleOwner == userId).AsQueryable();

            return await GetRevenueTicketDTO(query);
        }
        private async Task<RevenueTicketDTO> GetRevenueForStaff(int userId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).AsQueryable();

            return await GetRevenueTicketDTO(query);
        }
        private async Task<RevenueTicketDTO> GetRevenueTicketDTO(IQueryable<Ticket> query)
        {
            var listTicket = await query
                .Join(_context.Vehicles, ticket => ticket.VehicleId, vehicle => vehicle.Id, (ticket, vehicle) => new { ticket, vehicle })
                .Join(_context.Users, tv => tv.vehicle.VehicleOwner, user => user.Id, (tv, user) => new
                {
                    Id = tv.ticket.Id,
                    tv.ticket.PricePromotion,
                    tv.ticket.VehicleId,
                    tv.ticket.CreatedAt,
                    LiscenseVehicle = tv.vehicle.LicensePlate,
                    VehicleOwner = user.FullName, // Lấy tên chủ xe
                    TypeOfTicket = tv.ticket.TypeOfTicketNavigation.Description,
                    TypeOfPayment = tv.ticket.TypeOfPaymentNavigation.TypeOfPayment1,
                    Date = tv.ticket.TimeFrom
                })
                .Select(x => new TicketRevenue
                {
                    Id = x.Id,
                    PricePromotion = x.PricePromotion,
                    VehicleId = x.VehicleId,
                    CreatedAt = x.CreatedAt,
                    LiscenseVehicle = x.LiscenseVehicle,
                    VehicleOwner = x.VehicleOwner,
                    TypeOfTicket = x.TypeOfTicket,
                    DateTime = x.Date,
                    TypeOfPayment = x.TypeOfPayment
                })
                .ToListAsync();

            // Tính tổng giá trị vé
            var sumPriceTicket = await query.SumAsync(x => x.PricePromotion);

            // Kết hợp dữ liệu
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
            if (checkTicket.TypeOfPayment == 1 && checkTicket.Status.Equals("Đã thanh toán bằng tài khoản"))
            {
                throw new Exception("Ticket đã thanh toán");
            }

            _context.Tickets.Remove(checkTicket);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<ListTicketDTOs>> GetTicketByUserId(int userId)
        {
            try
            {

                if (userId <= 0)
                {
                    throw new Exception("Invalid user.");
                }
                var listTicket = await _context.Tickets.Where(x => x.UserId == userId).OrderByDescending(t => t.Id).ToListAsync();
                var mapper = _mapper.Map<List<ListTicketDTOs>>(listTicket);
                return mapper;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task updateTicketByTicketId(int ticketId, int userId, TicketUpdateDTOs ticket)
        {
            try
            {
                var ticketById = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket.PricePromotion == null)
                    throw new ArgumentException("PricePromotion cannot be null.");

                if (string.IsNullOrWhiteSpace(ticket.PointStart))
                    throw new ArgumentException("PointStart cannot be null or empty.");

                if (string.IsNullOrWhiteSpace(ticket.PointEnd))
                    throw new ArgumentException("PointEnd cannot be null or empty.");

                ticketById.Note = ticket.Note;
                ticketById.Description = ticket.Description;
                ticketById.PricePromotion = ticket.PricePromotion;
                ticketById.PointStart = ticket.PointStart;
                ticketById.PointEnd = ticket.PointEnd;
                ticketById.UpdateAt = DateTime.Now;
                ticketById.UpdateBy = userId;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task deleteTicketByTicketId(int id, int userId)
        {
            try
            {
                var ticketById = await _context.Tickets.FirstOrDefaultAsync(x => x.TypeOfPayment == Constant.TIEN_MAT && x.Id == id);
                if (ticketById == null)
                {
                    throw new Exception("Not found ticket valid");
                }
                _context.Tickets.Remove(ticketById);

                var pointUser = await _context.PointUsers.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.UserId == ticketById.UserId);
                var pointUserUpdate = new PointUser
                {
                    Points = pointUser.Points - (int?)((int?)ticketById.PricePromotion * Constant.TICH_DIEM),
                    PointsMinus = (int?)((int?)ticketById.PricePromotion * Constant.TICH_DIEM),
                    Date = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    PaymentId = null
                };
                _context.PointUsers.Add(pointUserUpdate);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task<RevenueTicketDTO> GetRevenueTicketDTOUpdate(IQueryable<Ticket> query)
        {
            var listTicket = await query
                .Join(_context.Vehicles, ticket => ticket.VehicleId, vehicle => vehicle.Id, (ticket, vehicle) => new { ticket, vehicle })
                .Join(_context.Users, tv => tv.vehicle.VehicleOwner, user => user.Id, (tv, user) => new
                {
                    Id = tv.ticket.Id,
                    tv.ticket.PricePromotion,
                    tv.ticket.VehicleId,
                    tv.ticket.CreatedAt,
                    LiscenseVehicle = tv.vehicle.LicensePlate,
                    VehicleOwner = user.FullName, // Lấy tên chủ xe
                    TypeOfTicket = tv.ticket.TypeOfTicketNavigation.Description,
                    TypeOfPayment = tv.ticket.TypeOfPaymentNavigation.TypeOfPayment1
                })
                .Select(x => new TicketRevenue
                {
                    Id = x.Id,
                    PricePromotion = x.PricePromotion,
                    VehicleId = x.VehicleId,
                    CreatedAt = x.CreatedAt,
                    LiscenseVehicle = x.LiscenseVehicle,
                    VehicleOwner = x.VehicleOwner,
                    TypeOfTicket = x.TypeOfTicket,
                    TypeOfPayment = x.TypeOfPayment
                })
                .ToListAsync();

            // Tính tổng giá trị vé
            var sumPriceTicket = await query.SumAsync(x => x.PricePromotion);

            // Kết hợp dữ liệu
            var combineResult = new RevenueTicketDTO
            {
                total = sumPriceTicket,
                listTicket = listTicket
            };

            return combineResult;
        }
        public async Task<RevenueTicketDTO> getRevenueTicketUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId, int userId)
        {
            try
            {
                if (startDate > endDate)
                {
                    throw new Exception("Start time must be earlier than or equal to end time.");
                }
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    var now = DateTime.Now;
                    startDate ??= new DateTime(now.Year, now.Month, 1);
                    endDate ??= new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                }
                if (userId <= 0)
                {
                    throw new Exception("Invalid user ID.");
                }
                var getInforUser = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Where(x => x.Id == userId).FirstOrDefault();
                if (getInforUser == null)
                {
                    throw new Exception("User not found.");
                }
                if (IsUserRole(getInforUser, "Staff"))
                {
                    return await GetRevenueForStaffUpdate(startDate, endDate, vehicleId);
                }
                throw new Exception("User role is not supported.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task<RevenueTicketDTO> GetRevenueForStaffUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            var query = _context.Tickets.Include(x => x.Vehicle).Where(x => x.TimeTo >= startDate && x.TimeTo <= endDate);
            if (vehicleId.HasValue && vehicleId != 0)
            {
                query = query.Where(x => x.VehicleId == vehicleId);
            }
            if (vehicleId == null)
            {
                query = query;
            }
            return await GetRevenueTicketDTOUpdate(query);
        }
    }
}

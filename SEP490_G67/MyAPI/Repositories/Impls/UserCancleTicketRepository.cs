using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.UserCancleTicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Net.WebSockets;

namespace MyAPI.Repositories.Impls
{
    public class UserCancleTicketRepository : GenericRepository<UserCancleTicket>, IUserCancleTicketRepository
    {
        private readonly IRequestRepository _requestRepository;
        public UserCancleTicketRepository(SEP490_G67Context context, IRequestRepository requestRepository) : base(context)
        {
            _requestRepository = requestRepository;
        }

        public async Task AddUserCancleTicket(AddUserCancleTicketDTOs addUserCancleTicketDTOs, int userId)
        {


            if (addUserCancleTicketDTOs.TicketId <= 0)
            {
                throw new Exception("Ticket ID must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(addUserCancleTicketDTOs.ReasonCancle))
            {
                throw new Exception("Reason for cancellation cannot be null or empty.");
            }

            if (userId <= 0)
            {
                throw new Exception("User ID must be greater than 0.");
            }
            try
            {
                var ticket = await _context.Tickets
                                   .FirstOrDefaultAsync(t => t.Id == addUserCancleTicketDTOs.TicketId && t.UserId == userId);

                if (ticket == null)
                {
                    throw new UnauthorizedAccessException("Ticket does not belong to the user.");
                }
                if(ticket.Status == "Hủy chuyến")
                {
                    throw new UnauthorizedAccessException("Ticket had cancle!");
                }

                DateTime dateTimeCancle = DateTime.Now.AddHours(-2);
                var listTicketId = await _context.Tickets.Where(x => x.UserId == userId && x.TimeFrom <= dateTimeCancle).ToListAsync();
                if (ticket.TimeFrom <= dateTimeCancle)
                {
                    throw new InvalidOperationException("The ticket cannot be canceled within 2 hours of departure.");
                }

                
                var inforTicketCancle = await (from t in _context.Tickets
                                               join p in _context.Payments
                                                 on t.Id equals p.TicketId
                                               where t.Id == addUserCancleTicketDTOs.TicketId
                                               select p).FirstOrDefaultAsync();

                if (ticket.TypeOfPayment == Constant.TIEN_MAT)
                {
                    ticket.Status = "Hủy chuyến";
                    var addCancleTicket = new UserCancleTicket
                    {
                        TicketId = addUserCancleTicketDTOs.TicketId,
                        ReasonCancle = addUserCancleTicketDTOs.ReasonCancle,
                        UserId = userId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                    };
                    var pointUserMinus = (int)ticket.Price * Constant.TICH_DIEM;

                    var pointUserById = _context.PointUsers.FirstOrDefault(x => x.UserId == userId);
                    if (pointUserById == null)
                    {
                        throw new NullReferenceException();
                    }
                    else
                    {
                        pointUserById.Points -= (int)pointUserMinus;
                        if (pointUserById.Points < 0) { pointUserById.Points = 0; }
                    }
                    
                    var pointUserCancleTicket = new PointUser
                    {
                        PointsMinus = (int) pointUserMinus,
                        UserId = userId,
                        Points = pointUserById.Points,
                        Date = pointUserById.Date,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdateAt = null,
                        UpdateBy = null,
                    };
                    _context.Tickets.Update(ticket);
                    _context.PointUsers.Add(pointUserCancleTicket);
                    _context.UserCancleTickets.Add(addCancleTicket);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // request đến staff
                    var RequestCancleTicket = new RequestCancleTicketDTOs
                    {
                        TicketId = inforTicketCancle.TicketId,
                        Description = addUserCancleTicketDTOs.ReasonCancle,
                        TypeId = Constant.CHUYEN_KHOAN,
                    };
                    await _requestRepository.createRequestCancleTicket(RequestCancleTicket, userId);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
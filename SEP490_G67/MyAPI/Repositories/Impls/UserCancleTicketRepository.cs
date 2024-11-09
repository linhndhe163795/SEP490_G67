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
            try
            {
                DateTime dateTimeCancle = DateTime.Now.AddHours(-2);
                var listTicketId = await _context.Tickets.Where(x => x.UserId == userId && x.TimeFrom <= dateTimeCancle).ToListAsync();
                if (!listTicketId.Any())
                {
                    throw new NullReferenceException("Không có vé của nào của user");
                }
                var ticketToCancel = listTicketId.FirstOrDefault(ticket => ticket.Id == addUserCancleTicketDTOs.TicketId);
                if (ticketToCancel == null)
                {
                    throw new NullReferenceException("Không có vé hợp lệ để hủy");
                }
                else
                {
                    ticketToCancel.Status = "Hủy chuyến";
                }
                var inforTicketCancle = await (from t in _context.Tickets join p in _context.Payments
                                                 on t.Id equals p.TicketId
                                                 where t.Id == addUserCancleTicketDTOs.TicketId
                                                 select p).FirstOrDefaultAsync();
                if (inforTicketCancle == null) 
                {
                    throw new NullReferenceException("Không tìm thấy ticket!");
                }
                else
                {
                    if(inforTicketCancle.TypeOfPayment == Constant.TIEN_MAT)
                    {
                        var addCancleTicket = new UserCancleTicket
                        {
                            TicketId = addUserCancleTicketDTOs.TicketId,
                            PaymentId = inforTicketCancle.PaymentId,
                            ReasonCancle = addUserCancleTicketDTOs.ReasonCancle,
                            UserId = userId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                        };
                        var pointUserMinus = (int) inforTicketCancle.Price * Constant.TICH_DIEM;

                        var pointUserById = _context.PointUsers.FirstOrDefault(x => x.UserId == userId);
                        if (pointUserById == null)
                        {
                            throw new NullReferenceException();
                        }
                        else
                        {
                            pointUserById.Points -= (int) pointUserMinus;
                            if (pointUserById.Points < 0) { pointUserById.Points = 0; }
                        }
                        var ticketPaymet = await (from t in _context.Tickets
                                                  join p in _context.Payments
                                                  on t.Id equals p.TicketId
                                                  where t.Id == addUserCancleTicketDTOs.TicketId
                                                  select p
                                                  ).FirstOrDefaultAsync();
                        if(ticketPaymet == null)
                        {
                            throw new NullReferenceException();
                        }
                        else
                        {
                            ticketPaymet.Price = 0;
                        }
                        var pointUserCancleTicket = new PointUser
                        {
                            PaymentId = ticketPaymet.PaymentId,
                            PointsMinus = (int)pointUserMinus,
                            UserId = userId,
                            Points = pointUserById.Points,
                            Date = pointUserById.Date,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdateAt = null,
                            UpdateBy = null,
                        };
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
               
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

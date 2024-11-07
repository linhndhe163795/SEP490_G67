using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.UserCancleTicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Net.WebSockets;

namespace MyAPI.Repositories.Impls
{
    public class UserCancleTicketRepository : GenericRepository<UserCancleTicket>, IUserCancleTicketRepository
    {
        public UserCancleTicketRepository(SEP490_G67Context context) : base(context)
        {

        }

        public async Task AddUserCancleTicket(AddUserCancleTicketDTOs addUserCancleTicketDTOs, int userId)
        {
            try
            {
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
                        var pointUserMinus = (double) inforTicketCancle.Price * Constant.TICH_DIEM;

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

                        _context.UserCancleTickets.Add(addCancleTicket);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // request đến staff

                        // staff liên hệ trực tiếp đến sđt

                        // refund qua tknh
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

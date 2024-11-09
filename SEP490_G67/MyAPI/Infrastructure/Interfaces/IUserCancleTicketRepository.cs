using MyAPI.DTOs.UserCancleTicketDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IUserCancleTicketRepository : IRepository<UserCancleTicket>
    {
        Task AddUserCancleTicket(AddUserCancleTicketDTOs addUserCancleTicketDTOs, int userId);
    }
}

using MyAPI.DTOs.PointUserDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPointUserRepository : IRepository<PointUser>
    {

        //Task AddPointUser(int paymentId);
        Task<PointUserDTOs> getPointUserById(int userId);
    }
}

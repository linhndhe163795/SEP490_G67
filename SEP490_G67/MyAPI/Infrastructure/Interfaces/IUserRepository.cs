using MyAPI.DTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        bool checkAccountExsit(UserRegisterDTO userRegisterDTO);
        Task<User> Register(UserRegisterDTO userRegisterDTO);
        Task<int> lastIdUser();

    }
}

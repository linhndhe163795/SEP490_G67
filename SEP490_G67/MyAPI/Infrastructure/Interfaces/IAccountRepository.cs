using MyAPI.DTOs.AccountDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IAccountRepository : IRepository<User>
    {

        Task<List<AccountListDTO>> GetListAccount();

        Task<AccountListDTO> GetDetailsUser(int id);

        Task<bool> DeteleAccount(int id);

        Task<bool> UpdateRoleOfAccount(int id, int newRoleId);

        Task<List<AccountRoleDTO>> GetListRole();

    }
}

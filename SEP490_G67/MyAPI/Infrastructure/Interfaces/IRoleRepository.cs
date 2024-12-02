using MyAPI.DTOs.AccountDTOs;
using MyAPI.DTOs.RoleDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<RoleListDTO>> GetListRole();


        Task<bool> AddRole(RoleAddDTO roleAddDTO);

        Task<bool> UpdateRole(int id, RoleAddDTO roleAddDTO);

        Task<bool> DeleteRole(int id);


    }
}

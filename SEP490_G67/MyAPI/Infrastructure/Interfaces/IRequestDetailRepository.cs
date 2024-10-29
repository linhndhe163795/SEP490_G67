using MyAPI.DTOs.RequestDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRequestDetailRepository : IRepository<RequestDetail>
    {
        Task<RequestDetail> CreateRequestDetailAsync(RequestDetailDTO requestDetailDto);
        Task<RequestDetail> UpdateRequestDetailAsync(int id, RequestDetailDTO requestDetailDto);
    }
}

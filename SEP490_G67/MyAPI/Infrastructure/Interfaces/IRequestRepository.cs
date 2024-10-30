using MyAPI.DTOs.RequestDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRequestRepository : IRepository<Request>
    {
        Task<Request> CreateRequestAsync(RequestDTO request);
        Task<Request> UpdateRequestAsync(int id, Request request);
    }
}

using MyAPI.DTOs.RequestDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRequestRepository : IRepository<Request>
    {


        Task<Request> CreateRequestVehicleAsync(RequestDTO requestDTO);

        Task<bool> UpdateRequestVehicleAsync(int requestId, Request request);

        Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync();

        Task<Request> GetRequestWithDetailsByIdAsync(int id);
        Task<Request> UpdateRequestRentCarAsync(int id, RequestDTO requestDTO);

        Task<Request> CreateRequestRentCarAsync(RequestDTO requestDTO);

        Task DeleteRequestDetailAsync(int requestId, int detailId);

        Task<bool> AcceptRequestAsync(int requestId);
        Task<bool> DenyRequestAsync(int requestId);

    }
}

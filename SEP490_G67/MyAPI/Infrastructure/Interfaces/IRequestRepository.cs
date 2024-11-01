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
        Task<Request> UpdateRequestAsync(int id, Request request, List<RequestDetail> requestDetails);
        Task<Request> CreateRequestAsync(Request request, List<RequestDetail> requestDetails);

        Task DeleteRequestDetailAsync(int requestId, int detailId);

    }
}

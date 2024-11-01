using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRequestRepository : IRepository<Request>
    {
        
        Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync();

        Task<Request> GetRequestWithDetailsByIdAsync(int id);
        Task<Request> UpdateRequestAsync(int id, Request request, List<RequestDetail> requestDetails);
        Task<Request> CreateRequestAsync(Request request, List<RequestDetail> requestDetails);

        Task DeleteRequestDetailAsync(int requestId, int detailId);

    }
}

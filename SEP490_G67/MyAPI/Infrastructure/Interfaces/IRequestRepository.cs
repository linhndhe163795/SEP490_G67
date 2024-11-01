using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRequestRepository : IRepository<Request>
    {
        //Task<Request> CreateRequestAsync(Request request);
        Task<Request> UpdateRequestAsync(int id, Request request);

        Task<Request> CreateRequestAsync(Request request, List<RequestDetail> requestDetails);
    }
}

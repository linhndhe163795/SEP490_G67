using MyAPI.DTOs.HistoryRentVehicle;
using MyAPI.DTOs.RequestDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IRequestRepository : IRepository<Request>
    {
        Task<Request> CreateRequestVehicleAsync(RequestDTO requestDTO);
        Task<bool> UpdateRequestVehicleAsync(int requestId, Request request);
        Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync();
        Task<Request> GetRequestWithDetailsByIdAsync(int id);
        Task<Request> UpdateRequestRentCarAsync(int id, RequestDTOForRentCar requestDTO);
        Task<Request> CreateRequestRentCarAsync(RequestDTOForRentCar requestDTO);
        Task DeleteRequestDetailAsync(int requestId, int detailId);
        //Task<bool> AcceptRequestAsync(int requestId);
        //Task<bool> DenyRequestAsync(int requestId);
        Task createRequestCancleTicket(RequestCancleTicketDTOs requestCancleTicketDTOs, int userId);
        Task<List<ResponeCancleTicketDTOs>> getListRequestCancle();
        Task updateStatusRequestCancleTicket(int requestId, int staffId);

        Task<bool> CreateRequestRentVehicleAsync(RentVehicleAddDTO rentVehicleAddDTO);

        Task<bool> CreateRequestRentDriverAsync(RequestDetailForRentDriver rentVehicleAddDTO);

        Task<bool> CreateRequestCovenient(ConvenientTripDTO convenientTripDTO);

        Task<bool> UpdateStatusRequestConvenient(int requestId, bool choose);
    }
}

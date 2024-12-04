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
        Task<bool> UpdateRequestRentCarAsync(int id, RequestDTOForRentCar requestDTO);
        Task<bool> CreateRequestRentCarAsync(RequestDTOForRentCar requestDTO);
        Task<RequestDetailDTO> GetRequestDetailByIdAsync(int requestId);
        Task DeleteRequestDetailAsync(int requestId, int detailId);
        Task createRequestCancleTicket(RequestCancleTicketDTOs requestCancleTicketDTOs, int userId);
        Task<List<ResponeCancleTicketDTOs>> getListRequestCancle();
        Task updateStatusRequestCancleTicket(int requestId, int staffId);

        Task<bool> CreateRequestRentVehicleAsync(RentVehicleAddDTO rentVehicleAddDTO);

        Task<bool> CreateRequestRentDriverAsync(RequestDetailForRentDriver rentVehicleAddDTO);

        Task<bool> CreateRequestCovenient(ConvenientTripDTO convenientTripDTO);

        Task<bool> UpdateStatusRequestConvenient(int requestId, bool choose);
        Task<List<RequestDTO>> getListRequestForUser(int userId);
        Task updateRequest(RequestDetailDTO requestDetailDTO);
        Task<List<RequestDTO>> GetListRequestForDriver(int driverId);
        Task<Request?> GetRequestByIdAsync(int id);
        Task DeleteRequestWithDetailsAsync(Request request);
        Task<List<RequestDTO>> GetAllRequestsWithUserNameAsync();

    }
}

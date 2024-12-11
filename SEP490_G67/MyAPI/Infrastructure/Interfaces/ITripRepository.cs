using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Models;
using System.Linq.Expressions;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface ITripRepository : IRepository<Trip>
    {
        Task<List<TripDTO>> GetListTrip();
        Task<List<TripVehicleDTO>> SreachTrip(string startPoint, string endPoint, string time);
        Task AddTrip(TripDTO trip, int userId);  
        Task UpdateTripById(int tripId, UpdateTrip trip, int userId);
        Task updateStatusTrip(int id, int userId);
        Task confirmAddValidEntryImport(List<TripImportDTO> validEntry);
        Task<List<TripDTO>> getListTripNotVehicle();
        Task<(List<Trip>, List<string>)> ImportExcel(Stream excelStream);
        Task<decimal> SearchVehicleConvenient(string startPoint, string endPoint, int typeOfTrip, string? promotion);
        Task<int> GetTicketCount(int tripId, DateTime dateTime);
        Task<List<ListCovenientStartEndDTO>> getListStartAndEndPoint();
        Task<TripDTO> GetTripById(int id);
        Task<List<StartPointDTO>> getListStartPoint();
        Task<List<EndPointDTO>> getListEndPoint();
        //Task<TripByIdDTO> getTripByTripId(int tripId);
        Task<int> getTripDetailsId(string pointStart, string pointEnd, TimeSpan timeStartPoint, TimeSpan timeEndPoint);
        Task confirmAddValidEntriesConvenience(List<TripImportDTO> validEntries);
    }
}

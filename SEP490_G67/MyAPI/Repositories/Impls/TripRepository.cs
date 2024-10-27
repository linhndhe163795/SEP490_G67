using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class TripRepository : GenericRepository<Trip>, ITripRepository
    {
        private readonly IMapper _mapper;

        public TripRepository(SEP490_G67Context _context, IMapper mapper) : base(_context)
        {
            _mapper = mapper;
        }

        public async Task<List<DriverTripDTO>> GetListTrip()
        {
            try
            {
                var tripList = await _context.Trips.ToListAsync();
                var mapperTrip = _mapper.Map<List<DriverTripDTO>>(tripList);
                return mapperTrip;
            }
            catch (Exception ex)
            {
                throw new Exception("GetListTrip: " + ex.Message);
            }
        }

        public async Task<List<TripVehicleDTO>> SreachTrip(string startPoint, string endPoint, string? time)
        {
            try
            {
                var timeSpan = TimeSpan.Parse(time);
                var searchTrip = from t in _context.Trips
                                 join tv in _context.VehicleTrips
                                 on t.Id equals tv.TripId
                                 join v in _context.Vehicles
                                 on tv.VehicleId equals v.Id
                                 join u in _context.Users on v.VehicleOwner equals u.Id 
                                 where t.PointStart.Contains(startPoint) && t.PointEnd.Contains(endPoint) && t.StartTime >= timeSpan
                                 group new {t,v,u} by new
                                 {
                                     t.Id,
                                     u.FullName,
                                     t.Description,
                                     t.PointStart,
                                     t.PointEnd,
                                     t.StartTime
                                 } into tripGroup
                                 select new TripVehicleDTO
                                 {
                                     Id = tripGroup.Key.Id,
                                     FullName = tripGroup.Key.FullName,
                                     Description = tripGroup.Key.Description,
                                     PointStart = tripGroup.Key.PointStart,
                                     PointEnd = tripGroup.Key.PointEnd,
                                     StartTime = tripGroup.Key.StartTime,
                                     listVehicle = tripGroup.Select(g => new VehicleDTO
                                     {
                                         LicensePlate = g.v.LicensePlate,
                                         NumberSeat = g.v.NumberSeat,
                                         VehicleTypeId = g.v.VehicleTypeId,
                                         Price = g.t.Price,
                                         
                                     }).OrderByDescending(v => v.LicensePlate).ToList()
                                 };
                var searchTripMapper = _mapper.Map<List<TripVehicleDTO>>(searchTrip);
                return searchTripMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("SreachTrip: " + ex.Message);
            }
        }
        public async Task AddTrip(TripDTO trip)
        {
            try
            {
                Trip addTrip = new Trip
                {
                    Name = trip.Name,
                    Description = trip.Description,
                    PointStart = trip.PointStart,
                    PointEnd = trip.PointEnd,
                    StartTime = trip.StartTime,
                    CreatedAt = DateTime.Now,
                    CreatedBy = 1,
                    Price = trip.Price,
                    UpdateAt = DateTime.Now,
                    UpdateBy = 1
                };
                _context.Add(addTrip);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("addTrips: " + ex.Message, ex);
            }
        }

        public Task AssgineTripToVehicle(int tripId, List<int> vehicleId)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("AssgineTripToVehicle:" + ex.Message);
            }
        }
    }
}

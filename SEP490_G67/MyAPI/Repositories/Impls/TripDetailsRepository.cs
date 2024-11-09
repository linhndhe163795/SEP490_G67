using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class TripDetailsRepository : GenericRepository<TripDetail>, ITripDetailsRepository
    {
        private readonly IMapper _mapper;
        public TripDetailsRepository(SEP490_G67Context context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<List<EndPointTripDetails>> EndPointTripDetailsById(int TripId)
        {
            try
            {
                var listEndPointTripDetails = await _context.TripDetails.Where(x => x.TripId == TripId && x.Status == true).ToListAsync();
                var listEndPointTripDetailsMapper = _mapper.Map<List<EndPointTripDetails>>(listEndPointTripDetails)
                                                    .GroupBy(x => new { x.PointEndDetails, x.TimeEndDetails })  
                                                    .Select(g => g.First())  
                                                    .ToList();
                return listEndPointTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("EndPointTripDetailsById: " + ex.Message);
            }
        }

        public async Task<List<StartPointTripDetails>> StartPointTripDetailsById(int TripId)
        {
            try
            {
                var listStartPointTripDetails = await _context.TripDetails.Where(x => x.TripId == TripId && x.Status == true).ToListAsync();
                var listStartPointTripDetailsMapper = _mapper.Map<List<StartPointTripDetails>>(listStartPointTripDetails);
                return listStartPointTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("EndPointTripDetailsById: " + ex.Message);
            }
        }

        public async Task<List<TripDetailsDTO>> TripDetailsByTripId(int TripId)
        {
            try
            {
                var listTripDetails = await _context.TripDetails.Where(x => x.TripId == TripId && x.Status == true).ToListAsync();
                var listTripDetailsMapper = _mapper.Map<List<TripDetailsDTO>>(listTripDetails);
                return listTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("TripDetailsByTripId: " + ex.Message);
            }
        }
    }
}

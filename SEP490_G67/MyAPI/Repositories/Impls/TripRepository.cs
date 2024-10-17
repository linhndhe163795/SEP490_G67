using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.TripDTOs;
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

        public async Task<List<TripDTO>> GetListTrip()
        {
            try
            {
                var tripList = await _context.Trips.ToListAsync();
                var mapperTrip = _mapper.Map<List<TripDTO>>(tripList);
                return mapperTrip;
            }
            catch (Exception ex) 
            {
                throw new Exception("GetListTrip: " + ex.Message);
            }
        }

        public Task<List<TripDTO>> SreachTrip(string startPoint, string endPoint, DateTime? dateTime)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("SreachTrip: " + ex.Message);
            }
        }
    }
}

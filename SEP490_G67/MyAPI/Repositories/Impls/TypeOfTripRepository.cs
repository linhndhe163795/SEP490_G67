using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.DriverDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyAPI.Repositories.Impls
{
    public class TypeOfTripRepository : GenericRepository<TypeOfTrip>, ITypeOfTripRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;

        public TypeOfTripRepository(
            SEP490_G67Context context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            GetInforFromToken tokenHelper)
            : base(context)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
        }

        public async Task<IEnumerable<TypeOfTrip>> GetAll()
        {
            try
            {
                var typeOfTrips = await _context.TypeOfTrips.ToListAsync();

                if (typeOfTrips == null || !typeOfTrips.Any())
                {
                    throw new KeyNotFoundException("No types of trips found.");
                }

                return typeOfTrips;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving types of trips: " + ex.Message);
            }
        }



    }
}

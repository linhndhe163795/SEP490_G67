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
    public class TypeOfRequestRepository : GenericRepository<TypeOfRequest>, ITypeOfRequestRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;

        public TypeOfRequestRepository(
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

        public async Task<IEnumerable<TypeOfRequest>> GetAll()
        {
            try
            {
                var typeOfRequests = await _context.TypeOfRequests.ToListAsync();

                if (typeOfRequests == null || !typeOfRequests.Any())
                {
                    throw new KeyNotFoundException("No types of requests found.");
                }

                return typeOfRequests;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving types of requests: " + ex.Message);
            }
        }



    }
}

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
    public class TypeOfDriverRepository : GenericRepository<TypeOfDriver>, ITypeOfDriverRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;

        public TypeOfDriverRepository(
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

        public async Task<TypeOfDriver> CreateTypeOfDriverAsync(UpdateTypeOfDriverDTO updateTypeOfDriverDto)
        {

            if (updateTypeOfDriverDto == null)
            {
                throw new ArgumentNullException(nameof(updateTypeOfDriverDto), "Type of Driver data cannot be null.");
            }
            
            var typeOfDriver = _mapper.Map<TypeOfDriver>(updateTypeOfDriverDto);

            typeOfDriver.CreatedBy = 1;
            typeOfDriver.CreatedAt = DateTime.UtcNow;

            _context.TypeOfDrivers.Add(typeOfDriver);
            await _context.SaveChangesAsync();

            return typeOfDriver;
        }

        public async Task<TypeOfDriver> UpdateTypeOfDriverAsync(int id, UpdateTypeOfDriverDTO updateTypeOfDriverDto)
        {


            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0.");
            }

            if (updateTypeOfDriverDto == null)
            {
                throw new ArgumentNullException(nameof(updateTypeOfDriverDto), "Type of Driver data cannot be null.");
            }
            var existingTypeOfDriver = await _context.TypeOfDrivers.FindAsync(id);
            if (existingTypeOfDriver == null)
            {
                throw new KeyNotFoundException("Type of Driver not found");
            }

            _mapper.Map(updateTypeOfDriverDto, existingTypeOfDriver);

            existingTypeOfDriver.UpdateBy = 1;
            existingTypeOfDriver.UpdateAt = DateTime.UtcNow;

            _context.TypeOfDrivers.Update(existingTypeOfDriver);
            await _context.SaveChangesAsync();

            return existingTypeOfDriver;
        }

        public async Task Delete(TypeOfDriver typeOfDriver, string token)
        {
            if (typeOfDriver == null)
            {
                throw new ArgumentNullException(nameof(typeOfDriver), "Type of Driver data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null or empty.");
            }
            int userId = _tokenHelper.GetIdInHeader(token);
            if (userId == -1)
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }

            _context.TypeOfDrivers.Remove(typeOfDriver);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TypeOfDriver>> GetAll()
        {
            var typeOfDrivers = await _context.TypeOfDrivers.ToListAsync();
            if (typeOfDrivers == null || !typeOfDrivers.Any())
            {
                throw new KeyNotFoundException("No types of drivers found.");
            }

            return typeOfDrivers;
        }

        public async Task<TypeOfDriver> Get(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0.");
            }
            var typeOfDriver = await _context.TypeOfDrivers.FindAsync(id);
            if (typeOfDriver == null)
            {
                throw new KeyNotFoundException("Type of Driver not found.");
            }

            return typeOfDriver;
        }


    }
}

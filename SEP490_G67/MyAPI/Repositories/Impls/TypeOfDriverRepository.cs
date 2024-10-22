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


            // Map DTO to Entity
            var typeOfDriver = _mapper.Map<TypeOfDriver>(updateTypeOfDriverDto);

            // Set thêm thông tin người tạo
            typeOfDriver.CreatedBy = 1;
            typeOfDriver.CreatedAt = DateTime.UtcNow;

            // Add the new type of driver to the context
            _context.TypeOfDrivers.Add(typeOfDriver);
            await _context.SaveChangesAsync();

            return typeOfDriver;
        }

        public async Task<TypeOfDriver> UpdateTypeOfDriverAsync(int id, UpdateTypeOfDriverDTO updateTypeOfDriverDto)
        {


            // Find existing TypeOfDriver by ID
            var existingTypeOfDriver = await _context.TypeOfDrivers.FindAsync(id);
            if (existingTypeOfDriver == null)
            {
                throw new KeyNotFoundException("Type of Driver not found");
            }

            // Update the existing entity with new values
            _mapper.Map(updateTypeOfDriverDto, existingTypeOfDriver);

            // Set thêm thông tin người cập nhật
            existingTypeOfDriver.UpdateBy = 1;
            existingTypeOfDriver.UpdateAt = DateTime.UtcNow;

            // Save changes
            _context.TypeOfDrivers.Update(existingTypeOfDriver);
            await _context.SaveChangesAsync();

            return existingTypeOfDriver;
        }

        public async Task Delete(TypeOfDriver typeOfDriver, string token)
        {
            // Lấy ID từ token để kiểm tra quyền hoặc log nếu cần
            int userId = _tokenHelper.GetIdInHeader(token);
            if (userId == -1)
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }

            // Remove the entity from the context
            _context.TypeOfDrivers.Remove(typeOfDriver);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TypeOfDriver>> GetAll()
        {
            // Return all types of drivers
            return await _context.TypeOfDrivers.ToListAsync();
        }

        public async Task<TypeOfDriver> Get(int id)
        {
            // Find type of driver by ID
            return await _context.TypeOfDrivers.FindAsync(id);
        }


    }
}

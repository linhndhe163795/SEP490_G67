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
    public class TypeOfPaymentRepository : GenericRepository<TypeOfPayment>, ITypeOfPaymentRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;

        public TypeOfPaymentRepository(
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

        public async Task<IEnumerable<TypeOfPayment>> GetAll()
        {
            try
            {
                var typeOfPayments = await _context.TypeOfPayments.ToListAsync();

                if (typeOfPayments == null || !typeOfPayments.Any())
                {
                    throw new KeyNotFoundException("No types of payment found.");
                }

                return typeOfPayments;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving types of payment: " + ex.Message);
            }
        }



    }
}

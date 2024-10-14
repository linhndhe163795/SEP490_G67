using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyAPI.Repositories.Impls
{
    public class TypeOfDriverRepository : GenericRepository<TypeOfDriver>, ITypeOfDriverRepository
    {
        public TypeOfDriverRepository(SEP490_G67Context _context, SendMail sendMail, IMapper mapper, HashPassword hashPassword, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper)
            : base(_context)
        {
        }

        
    }
}

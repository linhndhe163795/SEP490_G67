using AutoMapper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PointUserRepository : GenericRepository<PointUser>, IPointUserRepository
    {
        private readonly IMapper _mapper;
        public PointUserRepository(SEP490_G67Context _context, IMapper mapper) : base(_context)
        {
           _mapper = mapper;
        }

       
    }
}

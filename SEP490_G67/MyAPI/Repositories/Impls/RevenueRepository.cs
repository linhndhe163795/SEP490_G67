using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class RevenueRepository : IRevenueRepository
    {
        private readonly SEP490_G67Context _context;
        public RevenueRepository(SEP490_G67Context context)
        {
            _context = context;
        }
    }
}

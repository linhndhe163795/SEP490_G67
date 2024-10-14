using Microsoft.EntityFrameworkCore;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class DriverRepository : GenericRepository<Driver>, IDriverRepository
    {
        private readonly SEP490_G67Context _context;

        public DriverRepository(SEP490_G67Context context) : base(context)
        {
            _context = context;
        }

        public async Task<Driver> GetDriverWithVehicle(int id)
        {
            return await _context.Drivers.Include(d => d.Vehicles).FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<int> lastIdDriver()
        {
            int lastId = await _context.Users.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
            return lastId;
        }
    }

}

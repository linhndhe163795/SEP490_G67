using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(SEP490_G67Context context) : base(context)
        {

        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Linq.Expressions;

namespace MyAPI.Repositories.Impls
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly IMapper _mapper;
        private readonly HashPassword _hassPassword;
        public UserRepository(SEP490_G67Context _context, IMapper mapper, HashPassword hashPassword) : base(_context)
        {
            _mapper = mapper;
            _hassPassword = hashPassword;
        }

        public async Task<User> Register(UserRegisterDTO entity)
        {
            UserRegisterDTO userRegisterDTO = new UserRegisterDTO
            {
                Username = entity.Username,
                Password = _hassPassword.HashMD5Password(entity.Password),
                NumberPhone = entity.NumberPhone,
                Dob = entity.Dob,
                Email = entity.Email,
                Status = true,
            };
            var userMapper = _mapper.Map<User>(userRegisterDTO);
            await _context.AddAsync(userMapper);
            await base.SaveChange();
           
            return userMapper;
        }

        public bool checkAccountExsit(UserRegisterDTO userRegisterDTO)
        {
            var user = _context.Users.FirstOrDefault(x => x.Username == userRegisterDTO.Username || x.Email == userRegisterDTO.Email);
            return user != null;
        }

        public async Task<int> lastIdUser()
        {
            int lastId = await _context.Users.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
            return lastId;
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly SendMail _sendMail;
        private readonly IMapper _mapper;
        private readonly HashPassword _hassPassword;
        public UserRepository(SEP490_G67Context _context, SendMail sendMail, IMapper mapper, HashPassword hashPassword) : base(_context)
        {
            _mapper = mapper;
            _hassPassword = hashPassword;
            _sendMail = sendMail;
        }

        public async Task<User> Register(UserRegisterDTO entity)
        {

            var verifyCode = _sendMail.GenerateVerificationCode(4);
            SendMailDTO sendMailDTO = new()
            {
                FromEmail = "duclinh5122002@gmail.com",
                Password = "jetj haze ijdw euci",
                ToEmail = entity.Email,
                Subject = "Verify Code",
                Body = verifyCode,
            };
            if (await _sendMail.SendEmail(sendMailDTO))
            {
                UserRegisterDTO userRegisterDTO = new UserRegisterDTO
                {
                    Username = entity.Username,
                    Password = _hassPassword.HashMD5Password(entity.Password),
                    NumberPhone = entity.NumberPhone,
                    Dob = entity.Dob,
                    Email = entity.Email,
                    ActiveCode = verifyCode,
                    CreatedAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    Status = false,
                };
                var userMapper = _mapper.Map<User>(userRegisterDTO);
                await _context.AddAsync(userMapper);
                await base.SaveChange();
                return userMapper;
            }
            else
            {
                throw new Exception("Failed to send mail");
            }
        }

        public async Task<bool> checkAccountExsit(User user)
        {
            var userExit = await _context.Users.FirstOrDefaultAsync(x => x.Username == user.Username || x.Email == user.Email);
            return userExit != null;
        }

        public async Task<int> lastIdUser()
        {
            int lastId = await _context.Users.OrderByDescending(x => x.Id).Select(x => x.Id).FirstOrDefaultAsync();
            return lastId;
        }
        public async Task<bool> confirmCode(ConfirmCode code)
        {
            var acc = await _context.Users.FirstOrDefaultAsync(x => x.Email == code.Email && x.ActiveCode == code.Code);
            if (acc != null)
            {
                acc.Status = true;
                acc.ActiveCode = null;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> checkLogin(UserLoginDTO userLoginDTO)
        {
            var hashedPassword = _hassPassword.HashMD5Password(userLoginDTO.Password);
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                (x.Username == userLoginDTO.Username || x.Email == userLoginDTO.Email) && x.Password == hashedPassword && x.Status == true);
            return user != null;
        }
        public async Task ForgotPassword(ForgotPasswordDTO entity)
        {
            var acc = await _context.Users.FirstOrDefaultAsync(x => x.Email == entity.Email);
            if (acc != null)
            {
                var verifyCode = _sendMail.GenerateVerificationCode(6);
                SendMailDTO sendMailDTO = new()
                {
                    FromEmail = "duclinh5122002@gmail.com",
                    Password = "jetj haze ijdw euci",
                    ToEmail = entity.Email,
                    Subject = "Verify Code",
                    Body = verifyCode,
                };
                if (await _sendMail.SendEmail(sendMailDTO))
                {
                    acc.ActiveCode = verifyCode;
                    _context.Update(acc);
                    await base.SaveChange();
                }
                else
                {
                    return;
                }
            }
        }

        public async Task ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var acc = await _context.Users.FirstOrDefaultAsync(x => x.Email == resetPasswordDTO.Email && x.ActiveCode == resetPasswordDTO.Code);

            if (acc != null)
            {
                if (!string.IsNullOrEmpty(resetPasswordDTO.Password) && resetPasswordDTO.Password == resetPasswordDTO.ConfirmPassword)
                {
                    acc.Password = _hassPassword.HashMD5Password(resetPasswordDTO.Password);
                    acc.ActiveCode = null;
                    acc.UpdateBy = acc.Id;
                    base.Update(acc);
                }
                else
                {
                    throw new Exception("Passwords do not match or are empty.");
                }
            }
            else
            {
                throw new Exception("Invalid email or reset code.");
            }
        }
    }
}

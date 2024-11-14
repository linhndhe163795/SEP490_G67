using AutoMapper;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly HashPassword _hassPassword;
        private readonly IPointUserRepository _pointUserRepository;
        public UserRepository(SEP490_G67Context _context,IPointUserRepository pointUserRepository, SendMail sendMail, IMapper mapper, HashPassword hashPassword, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _mapper = mapper;
            _hassPassword = hashPassword;
            _sendMail = sendMail;
            _pointUserRepository = pointUserRepository;
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
                int userId = userMapper.Id;
                await _pointUserRepository.addNewPointUser(userId);
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
            try
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
            catch (Exception ex)
            {
                throw ex;

            }
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
                    await base.Update(acc);
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

        public async Task ChangePassword(ChangePasswordDTO changeEmailDTO)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == changeEmailDTO.CurrentEmail);
                var sendMailDTO = new SendMailDTO
                {
                    FromEmail = "duclinh5122002@gmail.com",
                    Password = "jetj haze ijdw euci",
                    ToEmail = changeEmailDTO.CurrentEmail,
                    Subject = "Đổi mật khẩu thành công",
                    Body = "Mật khẩu của bạn đã được đổi thành công."
                };

                bool isSent = await _sendMail.SendEmail(sendMailDTO);
                if (!isSent)
                {
                    throw new Exception("Cannot send notification.");
                }

                if (user == null)
                {
                    throw new Exception("Not exist user!");
                }


                var hashPassword = new HashPassword();
                string hashedCurrentPassword = hashPassword.HashMD5Password(changeEmailDTO.OldPassword);


                if (user.Password != hashedCurrentPassword)
                {
                    throw new Exception("Password is not correct");
                }


                user.Password = hashPassword.HashMD5Password(changeEmailDTO.NewPassword);
                user.UpdateAt = DateTime.UtcNow;
                user.UpdateBy = user.Id;


                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Something wrong when change email:" + ex.Message);
            }
        }

        public async Task<User> EditProfile(EditProfileDTO editProfileDTO)
        {
            try
            {
                // Lấy token từ header Authorization
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Lấy userId từ token
                int userId = _tokenHelper.GetIdInHeader(token);

                if (userId == -1)
                {
                    throw new Exception("Invalid user ID from token.");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception("User does not exist.");
                }

                // Cập nhật thông tin người dùng
                user.Username = editProfileDTO.Username;
                user.Email = editProfileDTO.Email;
                user.NumberPhone = editProfileDTO.NumberPhone;
                user.Avatar = editProfileDTO.Avatar;
                user.FullName = editProfileDTO.FullName;
                user.Address = editProfileDTO.Address;
                user.Dob = editProfileDTO.Dob;
                user.UpdateAt = DateTime.UtcNow;
                user.UpdateBy = userId;

                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong when updating user information: " + ex.Message);
            }
        }

        public async Task<UserLoginDTO> GetUserLogin(UserLoginDTO userLogin)
        {
            try
            {
                var hashedPassword = _hassPassword.HashMD5Password(userLogin.Password);
                var user = await _context.Users
                    .Include(x => x.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Where(x =>
                    (x.Username == userLogin.Username || x.Email == userLogin.Email) &&
                    x.Password == hashedPassword &&
                    x.Status == true)
                    .Select(x => new UserLoginDTO
                    {
                        Email = x.Email,
                        Id = x.Id,
                        NumberPhone = x.NumberPhone,
                        Password = x.Password,
                        RoleName = string.Join(", ", x.UserRoles.Select(ur => ur.Role.RoleName)),
                    })
                    .FirstOrDefaultAsync();
                return user != null ? user : throw new Exception();
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserLogin: " + ex.Message);
            }
        }

        public async Task<UserPostLoginDTO> getUserById(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                {
                    throw new NullReferenceException("User not found.");
                }

                var userMapper = _mapper.Map<UserPostLoginDTO>(user);
                userMapper.Role = user.UserRoles.FirstOrDefault()?.Role?.RoleName;

                return userMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("getUserById: " + ex.Message);
            }
        }


    }
}

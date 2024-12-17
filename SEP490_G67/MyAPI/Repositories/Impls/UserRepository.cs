using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleOwnerDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Text.RegularExpressions;

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
        private readonly GetInforFromToken _getInforFromToken;
        public UserRepository(GetInforFromToken getInforFromToken, SEP490_G67Context _context, IPointUserRepository pointUserRepository, SendMail sendMail, IMapper mapper, HashPassword hashPassword, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _getInforFromToken = getInforFromToken;
            _tokenHelper = tokenHelper;
            _mapper = mapper;
            _hassPassword = hashPassword;
            _sendMail = sendMail;
            _pointUserRepository = pointUserRepository;
        }

        public async Task<User> Register(UserRegisterDTO entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "User registration data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(entity.Username))
            {
                throw new Exception("Username cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(entity.Password))
            {
                throw new Exception("Password cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(entity.Email))
            {
                throw new Exception("Email cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(entity.NumberPhone))
            {
                throw new Exception("NumberPhone cannot be null or empty.");
            }
            if (!IsValidEmail(entity.Email))
            {
                throw new Exception("Email is invalid.");
            }
            if (!IsValidPhone(entity.NumberPhone))
            {
                throw new Exception("Phone is invalid");
            }
            if (!IsPasswordValid(entity.Password))
            {
                throw new Exception("Password is weak, please input password has length more than 6 charater");
            }
            if (entity.Dob == null)
            {
                throw new Exception("Date of Birth cannot be null.");
            }
            var verifyCode = _sendMail.GenerateVerificationCode(4);
            var sendMailDTO = new SendMailDTO
            {
                FromEmail = "duclinh5122002@gmail.com",
                Password = "jetj haze ijdw euci",
                ToEmail = entity.Email,
                Subject = "Xác thực OTP",
                Body = $@"
                Xin chào {entity.Username},

                Để hoàn tất quy trình đăng nhập hoặc đặt vé, vui lòng sử dụng mã OTP sau:

                {verifyCode}

                Nếu bạn không yêu cầu mã OTP, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi để được hỗ trợ.

                Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!"
            };

            if (await _sendMail.SendEmail(sendMailDTO))
            {
                entity.Password = _hassPassword.HashMD5Password(entity.Password);
                entity.ActiveCode = verifyCode;
                entity.CreatedAt = DateTime.Now;
                entity.UpdateAt = DateTime.Now;
                entity.Status = false;

                var userMapper = _mapper.Map<User>(entity);
                await _context.AddAsync(userMapper);
                await base.SaveChange();
                await _pointUserRepository.addNewPointUser(userMapper.Id);

                return userMapper;
            }
            else
            {
                throw new Exception("Failed to send verification email.");
            }
        }

        public bool IsValidEmail(string email)
        {
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern);
        }
        public bool IsValidPhone(string phone)
        {
            var phonePattern = @"^(03|05|07|08|09)\d{8}$";
            return Regex.IsMatch(phone, phonePattern);
        }
        public bool IsPasswordValid(string password)
        {
            return password.Length >= 6;
        }

        public async Task<bool> checkAccountExsit(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User data cannot be null.");
            }

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
            if (userLoginDTO == null)
            {
                throw new Exception("Login data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(userLoginDTO.Username))
            {
                throw new Exception("Username cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(userLoginDTO.Password))
            {
                throw new Exception("Password cannot be null or empty.");
            }

            var hashedPassword = _hassPassword.HashMD5Password(userLoginDTO.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == userLoginDTO.Username &&
                    x.Password == hashedPassword &&
                    x.Status == true);

            return user != null;
        }

        public async Task ForgotPassword(ForgotPasswordDTO entity)
        {
            if (entity == null)
            {
                throw new Exception("Forgot password data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(entity.Email))
            {
                throw new Exception("Email cannot be null or empty.");
            }

            var acc = await _context.Users.FirstOrDefaultAsync(x => x.Email == entity.Email);
            if (acc != null)
            {
                var verifyCode = _sendMail.GenerateVerificationCode(6);
                var sendMailDTO = new SendMailDTO
                {
                    FromEmail = "duclinh5122002@gmail.com",
                    Password = "jetj haze ijdw euci",
                    ToEmail = entity.Email,
                    Subject = "Xác thực OTP",
                    Body = $@"
                Xin chào {entity.Email},

                Để hoàn tất quy trình lấy lại mật khẩu, vui lòng sử dụng mã OTP sau:

                {verifyCode}

                Nếu bạn không yêu cầu mã OTP, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi để được hỗ trợ.

                Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!"
                };

                if (await _sendMail.SendEmail(sendMailDTO))
                {
                    acc.ActiveCode = verifyCode;
                    _context.Update(acc);
                    await base.SaveChange();
                }
                else
                {
                    throw new Exception("Failed to send verification email.");
                }
            }
            else
            {
                throw new Exception("Account not found with the provided email.");
            }
        }

        public async Task ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            if (resetPasswordDTO == null)
            {
                throw new ArgumentNullException(nameof(resetPasswordDTO), "Reset password data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(resetPasswordDTO.Code))
            {
                throw new Exception("Verification code cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(resetPasswordDTO.Password))
            {
                throw new Exception("Password cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(resetPasswordDTO.ConfirmPassword))
            {
                throw new Exception("Confirm password cannot be null or empty.");
            }

            if (resetPasswordDTO.Password != resetPasswordDTO.ConfirmPassword)
            {
                throw new Exception("Passwords do not match.");
            }

            var acc = await _context.Users.FirstOrDefaultAsync(x => x.Email == resetPasswordDTO.Email && x.ActiveCode == resetPasswordDTO.Code);
            if (acc != null)
            {
                acc.Password = _hassPassword.HashMD5Password(resetPasswordDTO.Password);
                acc.ActiveCode = null;
                acc.UpdateBy = acc.Id;
                await base.Update(acc);
            }
            else
            {
                throw new Exception("Invalid email or reset code.");
            }
        }

        public async Task ChangePassword(ChangePasswordDTO changeEmailDTO, int userId)
        {
            try
            {

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new Exception("Not exist user!");
                }
                if (string.IsNullOrEmpty(changeEmailDTO.NewPassword))
                {
                    throw new Exception("New password not null");
                }
                if (!IsPasswordValid(changeEmailDTO.NewPassword))
                {
                    throw new Exception("Password is weak, please input password has length more than 6 charater");
                }
                var hashPassword = new HashPassword();
                string hashedCurrentPassword = hashPassword.HashMD5Password(changeEmailDTO.OldPassword);

                if (user.Password != hashedCurrentPassword)
                {
                    throw new Exception("Password Current is not correct");
                }


                user.Password = hashPassword.HashMD5Password(changeEmailDTO.NewPassword);
                user.UpdateAt = DateTime.UtcNow;
                user.UpdateBy = user.Id;


                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<User> EditProfile(EditProfileDTO editProfileDTO)
        {

            if (string.IsNullOrWhiteSpace(editProfileDTO.Email))
            {
                throw new Exception("Email cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(editProfileDTO.NumberPhone))
            {
                throw new Exception("NumberPhone cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(editProfileDTO.FullName))
            {
                throw new Exception("FullName cannot be null or empty.");
            }
            if (!IsValidEmail(editProfileDTO.Email))
            {
                throw new Exception("Email is invalid.");
            }
            if (!IsValidPhone(editProfileDTO.NumberPhone))
            {
                throw new Exception("Phone is invalid");
            }
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

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


        public async Task<UserLoginDTO> GetUserLogin(UserLoginDTO userLogin)
        {
            if (userLogin == null)
            {
                throw new ArgumentNullException(nameof(userLogin), "Login data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(userLogin.Username) && string.IsNullOrWhiteSpace(userLogin.Email))
            {
                throw new Exception("Either Username or Email must be provided.");
            }

            if (string.IsNullOrWhiteSpace(userLogin.Password))
            {
                throw new Exception("Password cannot be null or empty.");
            }

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

                if (user == null)
                {
                    throw new Exception("Invalid username, email, or password.");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserLogin: " + ex.Message);
            }
        }


        public async Task<UserPostLoginDTO> getUserById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("User ID must be greater than 0.");
            }

            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var role = _tokenHelper.GetRoleFromToken(token);

                object user;

                if (role == "Driver")
                {
                    user = await _context.Drivers.FirstOrDefaultAsync(x => x.Id == id);
                }
                else
                {
                    user = await _context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .FirstOrDefaultAsync(x => x.Id == id);
                }

                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                UserPostLoginDTO userMapper;

                if (role == "Driver" && user is Driver driver)
                {
                    userMapper = _mapper.Map<UserPostLoginDTO>(driver);
                    userMapper.Role = "Driver";
                }
                else if (user is User appUser)
                {
                    userMapper = _mapper.Map<UserPostLoginDTO>(appUser);
                    userMapper.Role = appUser.UserRoles.FirstOrDefault()?.Role?.RoleName;
                }
                else
                {
                    throw new Exception("Unable to map user data.");
                }

                return userMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("getUserById: " + ex.Message);
            }
        }

        public async Task<List<UserPostLoginDTO>> getListVehicleOwner()
        {
            try
            {
                var vehicleOwner = await (from u in _context.Users
                                          join ur in _context.UserRoles
                                          on u.Id equals ur.UserId
                                          join r in _context.Roles
                                          on ur.RoleId equals r.Id
                                          where r.RoleName == "VehicleOwner"
                                          select new UserPostLoginDTO
                                          {
                                              Id = u.Id,
                                              Email = u.Email,
                                              FullName = u.FullName,
                                              NumberPhone = u.NumberPhone,
                                              Address = u.Address,
                                              Dob = u.Dob,
                                              Status = u.Status,
                                              Username = u.Username,
                                              Avatar = u.Avatar,
                                              Password = u.Password,
                                              Role = r.RoleName
                                          }).ToListAsync();
                if (vehicleOwner == null)
                {
                    throw new Exception("Not found any vehicle owner");
                }
                return vehicleOwner;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> RegisterVehicleOwner(VehicleOwnerDTO vehicleOwnerRegister, int staffId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vehicleOwnerRegister.Username))
                {
                    throw new Exception("Username cannot be null or empty.");
                }

                if (string.IsNullOrWhiteSpace(vehicleOwnerRegister.Password))
                {
                    throw new Exception("Password cannot be null or empty.");
                }
                if (string.IsNullOrWhiteSpace(vehicleOwnerRegister.Email))
                {
                    throw new Exception("Email cannot be null or empty.");
                }
                if (string.IsNullOrWhiteSpace(vehicleOwnerRegister.NumberPhone))
                {
                    throw new Exception("NumberPhone cannot be null or empty.");
                }
                if (!IsValidEmail(vehicleOwnerRegister.Email))
                {
                    throw new Exception("Email is invalid.");
                }
                if (!IsValidPhone(vehicleOwnerRegister.NumberPhone))
                {
                    throw new Exception("Phone is invalid");
                }
                if (!IsPasswordValid(vehicleOwnerRegister.Password))
                {
                    throw new Exception("Password is weak, please input password has length more than 6 charater");
                }
                if (vehicleOwnerRegister.Dob == null)
                {
                    throw new Exception("Date of Birth cannot be null.");
                }
                var userMapper = _mapper.Map<User>(vehicleOwnerRegister);
                if (await checkAccountExsit(userMapper))
                {
                    throw new Exception("User name or email is exist");
                }
                var vehicleOwner = new User
                {
                    Email = vehicleOwnerRegister.Email,
                    Username = vehicleOwnerRegister.Username,
                    Address = vehicleOwnerRegister.Address,
                    Avatar = vehicleOwnerRegister.Avatar,
                    FullName = vehicleOwnerRegister.FullName,
                    NumberPhone = vehicleOwnerRegister.NumberPhone,
                    Password = _hassPassword.HashMD5Password(vehicleOwnerRegister.Password),
                    Status = true,
                    Dob = vehicleOwnerRegister.Dob,
                    CreatedAt = DateTime.Now,
                    CreatedBy = staffId
            };
                _context.Users.Add(vehicleOwner);
                await _context.SaveChangesAsync();
                var userRole = new UserRole
                {
                    RoleId = 3,
                    UserId = vehicleOwner.Id
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
                return vehicleOwner;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
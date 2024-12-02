using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> checkAccountExsit(User user);
        Task<User> Register(UserRegisterDTO userRegisterDTO);
        Task<int> lastIdUser();
        Task<bool> confirmCode(ConfirmCode confirmCode);
        Task<bool> checkLogin(UserLoginDTO userLoginDTO);
        Task ForgotPassword(ForgotPasswordDTO forgotPassword);
        Task ResetPassword(ResetPasswordDTO resetPasswordDTO);
        Task ChangePassword(ChangePasswordDTO changePasswordDTO);
        Task<User> EditProfile( EditProfileDTO editProfileDTO);
        Task<UserLoginDTO> GetUserLogin(UserLoginDTO userLogin);
        Task<UserPostLoginDTO> getUserById(int id);



    }
}

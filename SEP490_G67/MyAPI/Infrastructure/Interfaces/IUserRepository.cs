using MyAPI.DTOs;
using MyAPI.DTOs.UserDTOs;
using MyAPI.DTOs.VehicleOwnerDTOs;
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
        Task ChangePassword(ChangePasswordDTO changePasswordDTO, int userId);
        Task<User> EditProfile( EditProfileDTO editProfileDTO);
        Task<UserLoginDTO> GetUserLogin(UserLoginDTO userLogin);
        Task<UserPostLoginDTO> getUserById(int id);
        Task<List<UserPostLoginDTO>> getListVehicleOwner();
        Task<User> RegisterVehicleOwner(VehicleOwnerDTO userRegisterDTO, int userId);

    }
}

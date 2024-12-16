using Microsoft.AspNetCore.Mvc;
using MyAPI.Models;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.DTOs.DriverDTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MyAPI.Helper;
using MyAPI.Repositories.Impls;
using ClosedXML;
using MyAPI.DTOs.PromotionDTOs;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverRepository _driverRepository;
        ITypeOfDriverRepository _typeOfDriverRepository;
        private readonly IMapper _mapper;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly Jwt _Jwt;


        public DriverController(IDriverRepository driverRepository, ITypeOfDriverRepository typeOfDriverRepository, IMapper mapper, Jwt jwt, GetInforFromToken getInforFromToken)
        {
            _driverRepository = driverRepository;
            _typeOfDriverRepository = typeOfDriverRepository;
            _mapper = mapper;
            _Jwt = jwt;
            _getInforFromToken = getInforFromToken;
        }
        [HttpPost("/loginDriver")]
        public async Task<IActionResult> loginDriver(LoginDriverDTO login)
        {
            try
            {
                if (await _driverRepository.checkLogin(login))
                {
                    var getDriverLogin = await _driverRepository.getDriverLogin(login);
                    var tokenString = _Jwt.CreateTokenDriver(getDriverLogin);
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = false,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(1)
                    };
                    string token = Request.Headers["Authorization"];
                   
                    var userId = _getInforFromToken.GetIdInHeader(tokenString);
                    var role = _getInforFromToken.GetRoleFromToken(tokenString);
                    var driverinfor = await _driverRepository.Get(userId);
                    return Ok(new
                    {
                        token = tokenString,
                        role = role,
                        id = userId,
                        userName = login.UserName ?? null,
                    });
                }
                else
                {
                    return NotFound("Incorrect Email or Password");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            var drivers = await _driverRepository.GetAll();
            var sortedDrivers = drivers.OrderByDescending(d => d.Id);
            var UpdateDriverDtos = _mapper.Map<IEnumerable<DriverDTO>>(sortedDrivers);

            return Ok(UpdateDriverDtos);
        }

        [Authorize(Roles = "Staff,Admin,Driver")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriverById(int id)
        {
            var driver = await _driverRepository.Get(id);
            if (driver == null)
            {
                return NotFound("Driver not found");
            }

            var UpdateDriverDto = _mapper.Map<DriverDTO>(driver);
            return Ok(UpdateDriverDto);
        }
        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> CreateDriver([FromBody] UpdateDriverDTO updateDriverDto)
        {
            if (updateDriverDto == null)
            {
                return BadRequest("Invalid driver data");
            }
            try
            {
                var driver = await _driverRepository.CreateDriverAsync(updateDriverDto);
                var createdDriverDto = _mapper.Map<UpdateDriverDTO>(driver);
                return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, createdDriverDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromForm] UpdateDriverDTO updateDriverDto)
        {
            
            if (updateDriverDto == null)
            {
                return BadRequest("Invalid driver data");
            }
            try
            {
                var existingDriver = await _driverRepository.UpdateDriverAsync(id, updateDriverDto);
                return Ok(existingDriver);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var driver = await _driverRepository.Get(id);
            if (driver == null)
            {
                return NotFound("Driver not found");
            }

            await _driverRepository.Delete(driver);
            return Ok("Driver deleted successfully");
        }
        [Authorize]
        [HttpPost("banDrive/{id}")]
        public async Task<IActionResult> banDriver(int id)
        {
            try
            {
                await _driverRepository.BanDriver(id);
                return Ok("update success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("driversWithoutVehicle")]
        public async Task<IActionResult> GetDriversWithoutVehicle()
        {
            var drivers = await _driverRepository.GetDriversWithoutVehicleAsync();
            return Ok(drivers);
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("send-mail-to-drivers-without-vehicle-for-rent")]
        public async Task<IActionResult> SendMailToDriverWithoutVehicle(int price)
        {
            try
            {
                await _driverRepository.SendEmailToDriversWithoutVehicle(price);

                return Ok("Emails sent successfully to all drivers without vehicles.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while sending emails: " + ex.Message);
            }
        }
        [HttpGet("listDriveDTO")]
        public async Task<IActionResult> GetDriveList()
        {
            try
            {
                var requests = await _driverRepository.getListDriverForVehicle();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Drive list not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Drive failed", Details = ex.Message });
            }
        }
    }
}

using AutoMapper;
using AutoMapper.Internal;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using OfficeOpenXml;
using System.IO.Packaging;

namespace MyAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {

        private readonly IVehicleRepository _vehicleRepository;
        private readonly ITripRepository _tripRepository;
        private readonly GetInforFromToken _inforFromToken;
        private readonly ServiceImport _serviceImport;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _getInforFromToken;
        public VehicleController(GetInforFromToken getInforFromToken, IVehicleRepository vehicleRepository, GetInforFromToken inforFromToken, ServiceImport serviceImport, IHttpContextAccessor httpContextAccessor, ITripRepository tripRepository)
        {
            _vehicleRepository = vehicleRepository;
            _inforFromToken = inforFromToken;
            _serviceImport = serviceImport;
            _httpContextAccessor = httpContextAccessor;
            _tripRepository = tripRepository;
            _getInforFromToken = getInforFromToken;
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("listVehicleType")]
        public async Task<IActionResult> GetVehicleType()
        {
            try
            {
                var requests = await _vehicleRepository.GetVehicleTypeDTOsAsync();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle Type not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Vehicle Typle failed", Details = ex.Message });
            }

        }
        [Authorize(Roles = "Staff, VehicleOwner, Driver")]
        [HttpGet("listVehicle")]
        public async Task<IActionResult> GetVehicleList()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _inforFromToken.GetIdInHeader(token);
                var role = _inforFromToken.GetRoleFromToken(token);
                var requests = await _vehicleRepository.GetVehicleDTOsAsync(userId, role);
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Vehicle list not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Get List Vehicle failed", Details = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("getInforVehicle/{id}")]
        public async Task<IActionResult> getVehicleDetailsById(int id)
        {
            try
            {
                var vehicleDetail = await _vehicleRepository.GetVehicleById(id);
                return Ok(vehicleDetail);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPost("addVehicle")]
        public async Task<IActionResult> AddVehicle(VehicleAddDTO vehicleAddDTO)
        {
            try
            {

                var isAdded = await _vehicleRepository.AddVehicleAsync(vehicleAddDTO);
                return Ok(new { Message = "Vehicle added successfully.", Vehicle = vehicleAddDTO });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle Add failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("addVehicleByStaff")]
        public async Task<IActionResult> AddVehicleByStaff(int requestID, bool isApprove)
        {
            try
            {
                var responseVehicle = await _vehicleRepository.AddVehicleByStaffcheckAsync(requestID, isApprove);
                if (responseVehicle)
                {
                    return Ok(new { Message = "Vehicle added successfully." });
                }
                else
                {
                    return Ok(new { Message = "Vehicle addition denied." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle By Staff failed", Details = ex.Message });
            }

        }

        [Authorize(Roles = "Staff")]
        [HttpPost("updateVehicleInformation/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleUpdateDTO updateDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _vehicleRepository.UpdateVehicleAsync(id, updateDTO);

                if (!result)
                {
                    return NotFound(new { Message = "Vehicle not found." });
                }

                return Ok(new { Message = "Vehicle updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

           
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("deleteVehicleByStatus/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id)
        {
            try
            {
                var delete = await _vehicleRepository.DeleteVehicleAsync(id);

                if (delete)
                {
                    return Ok(new { Message = "Vehicle delete successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Vehicle delete failed." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "DeleteVehicle Delete failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Driver,Staff")]
        [HttpGet("getStartPointTripFromVehicle/{vehicleId}")]
        public async Task<IActionResult> getStartPointTripFromVehicle(int vehicleId)
        {
            try
            {
                var listStartPoint = await _vehicleRepository.GetListStartPointByVehicleId(vehicleId);
                if (listStartPoint == null)
                {
                    return NotFound();
                }
                return Ok(listStartPoint);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
        [Authorize]
        [HttpGet("getEndPointTripFromVehicle/startPoint/{vehicleId}")]
        public async Task<IActionResult> getEndPointTripFromVehicle(int vehicleId,string startPoint)
        {
            try
            {
                var listEndPoint = await _vehicleRepository.GetListEndPointByVehicleId(vehicleId, startPoint);
                if (listEndPoint == null)
                {
                    return NotFound();
                }
                return Ok(listEndPoint);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("assignDriverForVehicle/{vehicleId}/{driverId}")]
        public async Task<IActionResult> AssignDriverForVehicle(int vehicleId, int driverId)
        {
            try
            {
                var isAssigned = await _vehicleRepository.AssignDriverToVehicleAsync(vehicleId, driverId);

                if (isAssigned)
                {
                    return Ok(new { Message = "Driver assigned to vehicle successfully." });
                }
                else
                {
                    return BadRequest(new { Message = "Assignment failed. Check if vehicle or driver exists." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AssignDriverForVehicle failed", Details = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("export_template_vehicel")]
        public async Task<IActionResult> exportTemplateVehicel()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateVehicle.xlsx");
                if
                    (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }
                byte[] fileBytes;
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateVehicle.xlsx");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("import_vehicle")]
        public async Task<IActionResult> importVehicel(IFormFile fileExcleVehicel)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var staffId = _inforFromToken.GetIdInHeader(token);
                var (validEntries, invalidEntries) = await _serviceImport.ImportVehicel(fileExcleVehicel, staffId);
                return Ok(new
                {
                    validEntries,
                    invalidEntries
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("confirmImportVehicle")]
        public async Task<IActionResult> confirmImportVehicle(List<VehicleImportDTO> validEntries)
        {
            var result = await _vehicleRepository.ConfirmAddValidEntryImportVehicle(validEntries);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok("Import confirmed successfully!");
        }
        [HttpGet("getNumberSeatAvaiable/{tripId}/{date}")]
        public async Task<IActionResult> GetNumberSeatAvailable(int tripId, DateTime date)
        {
            try
            {
                var trip = await _tripRepository.GetTripById(tripId);
                if (trip == null)
                {
                    return NotFound($"Trip with ID {tripId} was not found.");
                }

                if (!trip.StartTime.HasValue)
                {
                    return BadRequest("Trip StartTime is not available.");
                }

                // Combine the provided date with the trip start time
                var tripDateTime = new DateTime(date.Year, date.Month, date.Day).Add(trip.StartTime.Value);
                Console.WriteLine($"Trip DateTime: {tripDateTime}");

                // Retrieve the number of available seats
                var availableSeats = await _vehicleRepository.GetNumberSeatAvaiable(tripId, tripDateTime);
                return Ok(availableSeats);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred: {ex.Message}");

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("getLicenscePlate")]
        public async Task<IActionResult> getLicensePlateById()
        {
            try
            {
                var result = await _vehicleRepository.getLicensecePlate();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Driver")]
        [HttpGet("getVehicleByDriverId")]
        public async Task<IActionResult> getListVehicleByDriverId()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var driverId = _getInforFromToken.GetIdInHeader(token);
                var listVehicle = await _vehicleRepository.getVehicleByDriverId(driverId);
                return Ok(listVehicle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpGet("getVehicleByVehicleOwnerId")]
        public async Task<IActionResult> getListVehicleOfVehicleOwner()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var vehicleOwner = _getInforFromToken.GetIdInHeader(token);
                var listVehicle = await _vehicleRepository.getVehicleByVehicleOwner(vehicleOwner);
                return Ok(listVehicle);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("VehicleNoTrip")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAvailableVehiclesAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

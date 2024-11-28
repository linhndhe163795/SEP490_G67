﻿using AutoMapper;
using AutoMapper.Internal;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Repositories.Impls;

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
        [Authorize(Roles = "Staff")]
        [HttpGet("listVehicle")]
        public async Task<IActionResult> GetVehicleList()
        {
            try
            {
                var requests = await _vehicleRepository.GetVehicleDTOsAsync();
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
        //[Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPost("addVehicle")]
        public async Task<IActionResult> AddVehicle(VehicleAddDTO vehicleAddDTO, string? driverName)
        {
            try
            {

                var isAdded = await _vehicleRepository.AddVehicleAsync(vehicleAddDTO, driverName);
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
                    return BadRequest(new { Message = "Vehicle addition denied." });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "AddVehicle By Staff failed", Details = ex.Message });
            }

        }
        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPost("updateVehicle/{id}/{driverName}")]
        public async Task<IActionResult> UpdateVehicle(int id, string driverName)   
        {
            try
            {
                var checkUpdate = await _vehicleRepository.UpdateVehicleAsync(id, driverName);

                return Ok(new { Message = "Vehicle Update successfully." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "UpdateVehicle Update failed", Details = ex.Message });
            }

        }

        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPost("updateVehicleInformation/{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehicleUpdateDTO updateDTO)
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
        [Authorize(Roles = "Driver,Staff")]
        [Authorize]
        [HttpGet("getEndPointTripFromVehicle/{vehicleId}")]
        public async Task<IActionResult> getEndPointTripFromVehicle(int vehicleId)
        {
            try
            {
                var listEndPoint = await _vehicleRepository.GetListEndPointByVehicleId(vehicleId);
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
        [Authorize]
        [HttpPost("export_template_vehicel")]
        public async Task<IActionResult> exportTemplateVehicel()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {

                    var Vehicel = workbook.Worksheets.Add("Vehicle");
                    Vehicel.Cell(1, 1).Value = "Point Start Details";
                    Vehicel.Cell(1, 2).Value = "Point End Details";
                    Vehicel.Cell(1, 3).Value = "Time Start Details";
                    Vehicel.Cell(1, 4).Value = "Time End Details";
                    Vehicel.Cell(1, 7).Value = "Type_vehicel_id";
                    Vehicel.Cell(1, 8).Value = "Description";

                    Vehicel.Cell(2, 1).Value = "29";
                    Vehicel.Cell(2, 2).Value = "1";
                    Vehicel.Cell(2, 3).Value = "98B-01273";
                    Vehicel.Cell(2, 4).Value = "Thaco";

                    Vehicel.Cell(2, 7).Value = "1";
                    Vehicel.Cell(2, 8).Value = "Xe liên tỉnh";

                    Vehicel.Cell(3, 7).Value = "2";
                    Vehicel.Cell(3, 8).Value = "Xe tiện chuyến";

                    Vehicel.Cell(4, 7).Value = "3";
                    Vehicel.Cell(4, 8).Value = "Xe  du lịch";

                    var VehicelRow = Vehicel.Row(1);
                    VehicelRow.Style.Font.Bold = true;
                    VehicelRow.Style.Fill.BackgroundColor = XLColor.Gray;
                    VehicelRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    Vehicel.Columns().AdjustToContents();
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Seek(0, SeekOrigin.Begin);

                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateDataVehicels.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
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
        [HttpGet("getNumberSeatAvaiable/{tripId}")]
        public async Task<IActionResult> GetNumberSeatAvailable(int tripId)
        {
            try
            {
                var date = _httpContextAccessor?.HttpContext.Session.GetString("date");
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
                {
                    var trip = await _tripRepository.GetTripById(tripId);
                    if (trip != null)
                    {
                        if (trip.StartTime.HasValue)
                        {
                            var dateTime = parsedDate.Date.Add(trip.StartTime.Value);
                            Console.WriteLine($"DateTime: {dateTime}");

                            var count = await _vehicleRepository.GetNumberSeatAvaiable(tripId, dateTime);
                            return Ok(count);
                        }
                        else
                        {
                            return BadRequest("Trip StartTime is not available.");
                        }
                    }
                    else
                    {
                        return NotFound($"Trip with ID {tripId} was not found.");
                    }
                }
                else
                {
                    return BadRequest("Invalid or missing date in session.");
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                Console.WriteLine($"Error: {ex.Message}");
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
    }
}

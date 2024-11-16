using AutoMapper;
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
        private readonly GetInforFromToken _inforFromToken;
        private readonly ServiceImport _serviceImport;
        public VehicleController(IVehicleRepository vehicleRepository, GetInforFromToken inforFromToken, ServiceImport serviceImport)
        {
            _vehicleRepository = vehicleRepository;
            _inforFromToken = inforFromToken;
            _serviceImport = serviceImport;
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

        [Authorize(Roles = "Staff, VehicleOwner")]
        [HttpPost("addVehicle")]
        public async Task<IActionResult> AddVehicle(VehicleAddDTO vehicleAddDTO, string driverName)
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
        [HttpPut("updateVehicle/{id}/{driverName}")]
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
        [Authorize(Roles = "Staff")]
        [HttpDelete("deleteVehicleByStatus/{id}")]
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
        [Authorize]
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
        [HttpPut("assignDriverForVehicle/{vehicleId}/{driverId}")]
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
        [HttpGet("getNumberSeatAvaiable")]
        public async Task<IActionResult> getNumberSeatAvaiable(int vehicelId)
        {
            try
            {
                var count = await _vehicleRepository.GetNumberSeatAvaiable(vehicelId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

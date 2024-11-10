using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ServiceImport _serviceImport;

        public TripController(ITripRepository tripRepository, ServiceImport serviceImport, IHttpContextAccessor httpContextAccessor, GetInforFromToken getInforFromToken)
        {
            _tripRepository = tripRepository;
            _httpContextAccessor = httpContextAccessor;
            _getInforFromToken = getInforFromToken;
            _serviceImport = serviceImport;
        }
        [Authorize(Roles = "Staff")]
        [HttpGet]
        public async Task<IActionResult> GetListTrip()
        {
            try
            {
                var listTrip = await _tripRepository.GetListTrip();
                if (listTrip == null)
                {
                    return NotFound("Not found any trip");
                }
                else
                {
                    return Ok(listTrip);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetListTrip: " + ex.Message);
            }
        }
        [HttpGet("searchTrip/startPoint/endPoint/time")]
        public async Task<IActionResult> searchTrip(string startPoint,  string endPoint, DateTime time)
        {

            try
            {
                var timeonly = time.ToString("HH:ss:mm");
                var date = time.ToString();
                var searchTrip = await _tripRepository.SreachTrip(startPoint, endPoint, timeonly);
                _httpContextAccessor?.HttpContext?.Session.SetString("date", date);
                if (searchTrip == null) return NotFound("Not found trip");
                return Ok(searchTrip);
            }
            catch (Exception ex)
            {
                return BadRequest("searchTripAPI: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("addTrip")]
        public async Task<IActionResult> addTrip(TripDTO trip, int vehicleId)
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
                var userId = _getInforFromToken.GetIdInHeader(token);
                await _tripRepository.AddTrip(trip, vehicleId, userId);

                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download_template_trip")]
        public IActionResult DownloadTemplateTrip()
        {
            using (var workbook = new XLWorkbook())
            {
                // Tạo sheet cho Trip
                var tripSheet = workbook.Worksheets.Add("Trip");
                tripSheet.Cell(1, 1).Value = "name";
                tripSheet.Cell(1, 2).Value = "start_time";
                tripSheet.Cell(1, 3).Value = "description";
                tripSheet.Cell(1, 4).Value = "price";
                tripSheet.Cell(1, 5).Value = "point_start";
                tripSheet.Cell(1, 6).Value = "point_end";
                tripSheet.Cell(2, 1).Value = "Trip to Ninh Bình";
                tripSheet.Cell(2, 2).Value = "08:00:00";
                tripSheet.Cell(2, 3).Value = "Mỹ Đình - Ninh Bình";
                tripSheet.Cell(2, 4).Value = "80000";
                tripSheet.Cell(2, 5).Value = "Mỹ Đình";
                tripSheet.Cell(2, 6).Value = "Ninh Bình";
                var tripHeaderRow = tripSheet.Row(1);
                tripHeaderRow.Style.Font.Bold = true;
                tripHeaderRow.Style.Fill.BackgroundColor = XLColor.Gray;
                tripHeaderRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateDataTrip.xlsx");
                }
            }
        }
        [HttpGet("download_template_tripDetails")]
        public IActionResult DownloadTemplateTripDetails()
        {
            using (var workbook = new XLWorkbook())
            {
                // Tạo sheet cho Trip
                var tripSheet = workbook.Worksheets.Add("TripDetail");
                tripSheet.Cell(1, 1).Value = "Point Start Details";
                tripSheet.Cell(1, 2).Value = "Point End Details";
                tripSheet.Cell(1, 3).Value = "Time Start Details";
                tripSheet.Cell(1, 4).Value = "Time End Details";
              
                tripSheet.Cell(2, 1).Value = "Bến Xe Mỹ Đình";
                tripSheet.Cell(2, 2).Value = "Bến Xe Ninh Bình";
                tripSheet.Cell(2, 3).Value = "08:00:00";
                tripSheet.Cell(2, 4).Value = "11:00:00";

                tripSheet.Cell(3, 1).Value = "Big C Thăng Long";
                tripSheet.Cell(3, 2).Value = "Bến Xe Ninh Bình";
                tripSheet.Cell(3, 3).Value = "08:15:00";
                tripSheet.Cell(3, 4).Value = "11:00:00";

                var tripHeaderRow = tripSheet.Row(1);
                tripHeaderRow.Style.Font.Bold = true;
                tripHeaderRow.Style.Fill.BackgroundColor = XLColor.Gray;
                tripHeaderRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tripSheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateDataTripDetails.xlsx");
                }
            }
        }
            [Authorize(Roles ="Staff")]
            [HttpPost("importTrip")]
            public async Task<IActionResult> importTrip(IFormFile fileExcelTrip)
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
                var staffId = _getInforFromToken.GetIdInHeader(token);
                var (validEntries, invalidEntries) = await _serviceImport.ImportTrip(fileExcelTrip, staffId);
                return Ok(new
                {
                    validEntries,
                    invalidEntries
                });
            }
        [Authorize(Roles = "Staff")]
        [HttpPost("confirmImportTrip")]
        public async Task<IActionResult> ConfirmImportTrip([FromBody] List<Trip> validEntries)
        {
            if (validEntries == null || !validEntries.Any())
            {
                return BadRequest("No valid entries to import.");
            }
            await _tripRepository.confirmAddValidEntryImport(validEntries);
            return Ok("Successfully imported valid entries.");
        }

        [Authorize(Roles = "Staff")]
        [HttpPut("updateTrip/{id}")]
        public async Task<IActionResult> updateTrip(int id, TripDTO tripDTO)
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
                var userId = _getInforFromToken.GetIdInHeader(token);

                await _tripRepository.UpdateTripById(id, tripDTO, userId);
                return Ok(tripDTO);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("updateStatusTrip/{id}")]
        public async Task<IActionResult> updateStatusTrip(int id)
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
                var userId = _getInforFromToken.GetIdInHeader(token);
                await _tripRepository.updateStatusTrip(id, userId);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("listTripNotVehicle")]
        public async Task<IActionResult> getListTripNotVehicel()
        {
            try
            {
                var listTripNotHaveVehicel = await _tripRepository.getListTripNotVehicle();
                return Ok(listTripNotHaveVehicel);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using MyAPI.Repositories.Impls;
using OfficeOpenXml;

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
        // xe liên tỉnh
        [HttpGet("searchTrip/startPoint/endPoint/time")]
        public async Task<IActionResult> SearchTrip(string startPoint, string endPoint, DateTime time)
        {
            try
            {
                var timeonly = time.ToString("HH:mm:ss");
                var searchTrip = await _tripRepository.SreachTrip(startPoint, endPoint, timeonly);

                if (searchTrip == null)
                {
                    return NotFound("Not found trip");
                }

                return Ok(searchTrip);
            }
            catch (Exception ex)
            {
                return BadRequest("searchTripAPI: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("addTrip")]
        public async Task<IActionResult> addTrip(TripDTO trip)
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
                await _tripRepository.AddTrip(trip, userId);

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
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "TemplateTrip.xlsx");
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
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TemplateDataTrip.xlsx");
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("importTrip/{typeOfTrip}")]
        public async Task<IActionResult> importTrip(IFormFile fileExcelTrip, int typeOfTrip)
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
            var (validEntries, invalidEntries) = await _serviceImport.ImportTrip(fileExcelTrip, staffId, typeOfTrip);
            return Ok(new
            {
                validEntries,
                invalidEntries
            });
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("confirmImportTrip")]
        public async Task<IActionResult> ConfirmImportTrip([FromBody] List<TripImportDTO> validEntries)
        {
            if (validEntries == null || !validEntries.Any())
            {
                return BadRequest("No valid entries to import.");
            }
            await _tripRepository.confirmAddValidEntryImport(validEntries);
            return Ok("Successfully imported valid entries.");
        }

        [Authorize(Roles = "Staff")]
        [HttpPost("updateTrip/{id}")]
        public async Task<IActionResult> updateTrip(int id, UpdateTrip tripDTO)
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
        [HttpPost("updateStatusTrip/{id}")]
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

        [HttpPost("import")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            using (var stream = file.OpenReadStream())
            {
                var (validRows, errorRows) = await _tripRepository.ImportExcel(stream);

                return Ok(new
                {
                    SuccessRows = validRows.Count,
                    ErrorRows = errorRows
                });
            }
        }

        [HttpGet("searchTripForConvenient/{startPoint}/{endPoint}/{typeOfTrip}")]
        public async Task<IActionResult> SearchTripForConvenient(string startPoint, string endPoint, int typeOfTrip, string? promotion)
        {
            try
            {
                var price = await _tripRepository.SearchVehicleConvenient(startPoint, endPoint, typeOfTrip, promotion);

                if (price == 0)
                {
                    return NotFound(new { Message = "No trips found for the specified start and end points." });
                }

                return Ok(new { Price = price });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { Message = "An unexpected error occurred.", Detail = ex.Message });
            }
        }

        [HttpGet("listConvenientStartEnd")]
        public async Task<IActionResult> GetListconvenient()
        {
            try
            {
                var requests = await _tripRepository.getListStartAndEndPoint();
                if (requests != null)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("start and end point list not found");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "GetListconvenient failed", Details = ex.Message });
            }
        }
        [HttpGet("getListStartPoint")]
        public async Task<IActionResult> getListStartPoint()
        {
            try
            {
                var listStart = await _tripRepository.getListStartPoint();
                if (listStart == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(listStart);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getListEndPoint")]
        public async Task<IActionResult> getListEndPoint()
        {
            try
            {
                var listEnd = await _tripRepository.getListEndPoint();
                if (listEnd == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(listEnd);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("getListTripById/{id}")]
        public async Task<IActionResult> getTripById(int id)
        {
            try
            {
                var tripId = await _tripRepository.GetTripById(id);
                if (tripId == null)
                {
                    return NotFound("Not found trip");
                }
                return Ok(tripId);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("gettripDetailsId/{pointStart}/{pointEnd}/{dateStartPoint}/{dateEndPoint}")]
        public async Task<IActionResult> getTripDetailsId(string pointStart, string pointEnd, TimeSpan dateStartPoint, TimeSpan dateEndPoint)
        {
            try
            {
                var tripDetailsId = await _tripRepository.getTripDetailsId(pointStart, pointEnd, dateStartPoint, dateEndPoint);
                return Ok(tripDetailsId);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

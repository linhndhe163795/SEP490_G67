using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MyAPI.Helper
{
    public class ServiceImport
    {
        private readonly SEP490_G67Context _context;
        public ServiceImport(SEP490_G67Context context)
        {
            _context = context;
        }
        public bool checkFile (string path){
            string extension = Path.GetExtension(path); 
            return extension == ".xls" || extension == ".xlsx";
        }
        private bool IsValidTrip(TripImportDTO trip)
        {
            if (string.IsNullOrEmpty(trip.Name) ||
                trip.StartTime == default ||
                trip.Price <= 0 || trip.Price == null ||
                string.IsNullOrEmpty(trip.PointStart) ||
                string.IsNullOrEmpty(trip.PointEnd)  ||
                string.IsNullOrEmpty(trip.LicensePlate))

            {
                return false;
            }
            return true;
        }
        private async Task<bool> checkTypeVehicle(int? typeVehicel)
        {
            var typeOfVehicel =  await _context.VehicleTypes.FirstOrDefaultAsync(x => x.Id == typeVehicel);
            if (typeOfVehicel == null)
            {
                return false;
            }
            return true;
        }
        private bool IsValidVehicel(VehicleImportDTO vehicel)
        {
            if (vehicel.NumberSeat <= 0 ||
                vehicel.VehicleTypeId == null ||
                string.IsNullOrEmpty(vehicel.LicensePlate))
            {
                return false;
            }
            return true;
        }
        public async Task<(List<TripImportDTO> validEntries, List<TripImportDTO> invalidEntries)> ImportTrip(IFormFile excelFile, int staffId, int typeOfTrip)
        {
            string path = Path.GetFileName(excelFile.FileName); 
            if (!checkFile(path)) 
            {
                throw new Exception("File không hợp lệ"); 
            }
            var validEntries = new List<TripImportDTO>(); 
            var invalidEntries = new List<TripImportDTO>();
            var existingLicensePlates = await _context.Vehicles.Select(v => v.LicensePlate).ToListAsync();

            var licensePlateSet = new HashSet<string>(existingLicensePlates);
            using (var stream = new MemoryStream()) 
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;
                using (var workbook = new XLWorkbook(stream)) 
                {
                    var tripSheets = workbook.Worksheet("Trip");
                    foreach (var row in tripSheets.RowsUsed().Skip(1)) 
                    {
                        var trip = new TripImportDTO
                        {
                            Name = row.Cell(1).GetValue<string>(),
                            Description = row.Cell(3).GetValue<string>(),
                            PointStart = row.Cell(5).GetValue<string>(),
                            PointEnd = row.Cell(6).GetValue<string>(),
                            LicensePlate = row.Cell(7).GetValue<string>(),
                            TypeOfTrip = Constant.CHUYEN_DI_LIEN_TINH,
                            Status = true,
                            CreatedAt = DateTime.Now, CreatedBy = staffId,
                        };

                        if (!TimeSpan.TryParse(row.Cell(2).GetString(), out var parsedStartTime))
                        {
                            trip.ErrorMessages.Add("Invalid StartTime format in row : " + row.ToString() + " col: " + row.Cell(2).Value);
                        }
                        else
                        {
                            trip.StartTime = parsedStartTime;
                        }
                        if (!int.TryParse(row.Cell(4).GetString(), out var parsedPrice) || parsedPrice <= 0)
                        {
                            trip.ErrorMessages.Add("Invalid or negative Price in row: " + row.ToString() + " col: " + row.Cell(4).Value);
                        }
                        else
                        {
                            trip.Price = parsedPrice;
                        }
                        if (!licensePlateSet.Contains(trip.LicensePlate))
                        {
                            trip.ErrorMessages.Add($"License plate '{trip.LicensePlate}' does not exist in row: " + row.ToString());
                        }
                        for (int col = 8; col <= tripSheets?.LastColumnUsed()?.ColumnNumber(); col += 2) 
                        {
                            var pointDetail = row.Cell(col).GetValue<string>();
                            var pointEnd = row.Cell(col + 3).GetValue<string>();
                            if (TimeSpan.TryParse(row.Cell(col + 1).GetString(), out var pointTimeDetails))
                            {
                                
                                if (!string.IsNullOrEmpty(pointDetail) && !string.IsNullOrEmpty(pointEnd) )
                                {
                                    trip.PointStartDetail[pointDetail] = pointTimeDetails;
                                }
                                if (string.IsNullOrEmpty(pointEnd))
                                {
                                    var pointEndDetail = row.Cell(col).GetValue<string>();
                                    if (!trip.PointEndDetail.Any() && trip.PointStartDetail.Count > 0)
                                    {
                                        trip.PointEndDetail[pointEndDetail] = pointTimeDetails;
                                    }
                                }
                            }
                            if (pointTimeDetails == TimeSpan.Zero && !string.IsNullOrEmpty(pointDetail))
                            {
                                trip.ErrorMessages.Add("Incorrect format time in row: " + row.ToString() + " col: " + col);
                            }

                        }
                        if (trip.PointStartDetail.Count == 0 || trip.PointEndDetail.Count == 0)
                        {
                            trip.ErrorMessages.Add("PointStartDetail or PointEndDetail are missing in row: " + row.ToString() + " col: ");
                        }
                        if (IsValidTrip(trip) && trip.ErrorMessages.Count == 0) 
                        { 
                            validEntries.Add(trip); 
                        }
                        else
                        {
                            invalidEntries.Add(trip);
                        } 
                    }
                } 
            }
            return (validEntries, invalidEntries);
        }
        public async Task<(List<VehicleImportDTO> validEntries, List<VehicleImportDTO> invalidEntries)> ImportVehicel(IFormFile excelFile, int staffId)
        {
           
            if(excelFile == null)
            {
                throw new Exception("File is required");
            }
            string path = Path.GetFileName(excelFile.FileName);
            if (!checkFile(path))
            {
                throw new Exception("File is incorrect format");
            }
            var validEntries = new List<VehicleImportDTO>();
            var invalidEntries = new List<VehicleImportDTO>();
            var existingLicensePlates = await _context.Vehicles.Select(v => v.LicensePlate).ToListAsync();
            var vehicleOwners = await _context.Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                            .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "VehicleOwner"))
                            .Select(u => u.Username)
                            .ToListAsync();
            var licensePlateTracker = new Dictionary<string, List<VehicleImportDTO>>();
            var licensePlateSet = new HashSet<string>(existingLicensePlates);
            var vehicleOwnerSet = new HashSet<string>(vehicleOwners);
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var tripSheets = workbook.Worksheet("Vehicle");
                    foreach (var row in tripSheets.RowsUsed().Skip(1))
                    {
                        var userNameVehicleOwner = row.Cell(5).GetValue<string>();
                        var vehicleOwnerId = await _context.Users.Where(x => x.Username == userNameVehicleOwner).Select(x => x.Id).FirstOrDefaultAsync();
                        var driverUserName = row.Cell(6).GetValue<string>();
                        var driverId= await _context.Drivers.Where(x => x.UserName == driverUserName).Select(x => x.Id).FirstOrDefaultAsync();
                        
                        var vehicle = new VehicleImportDTO
                        {
                            NumberSeat = row.Cell(1).GetValue<Int32>(),
                            VehicleTypeId = row.Cell(2).GetValue<Int32>(),
                            LicensePlate = row.Cell(3).GetValue<string>(),
                            Description = row.Cell(4).GetValue<string>(),
                            VehicleOwner = vehicleOwnerId,
                            DriverId = driverId,
                            Status = true,
                        };
                        var errors = new List<string>();
                        if (driverId <= 0 && !string.IsNullOrWhiteSpace(driverUserName))
                        {
                            errors.Add("Invalid Driver Name in row :" +row);
                        }
                        if (licensePlateSet.Contains(vehicle.LicensePlate))
                        {
                            errors.Add("Duplicate license plate in row :" + row);
                        }
                        if (!Regex.IsMatch(vehicle.LicensePlate, @"^\d{2}[A-Z]-\d{5}$"))
                        {
                            errors.Add($"Invalid License Plate format '{vehicle.LicensePlate}' in row: {row.RowNumber()}");
                        }
                        if (licensePlateTracker.ContainsKey(vehicle.LicensePlate))
                        {
                            errors.Add($"Duplicate License Plate '{vehicle.LicensePlate}' in row: {row.RowNumber()}");
                        }

                        // Kiểm tra VehicleOwner
                        if (!vehicleOwnerSet.Contains(userNameVehicleOwner))
                        {
                            errors.Add("Invalid VehicleOwner in row :" + row);
                        }
                        if (!await checkTypeVehicle(vehicle.VehicleTypeId))
                        {
                            errors.Add("Invalid VehicleTypeId  in row: " +row);
                        }

                        if (errors.Count > 0)
                        {
                            vehicle.Errors = errors;
                            invalidEntries.Add(vehicle);
                        }
                        else
                        {
                            validEntries.Add(vehicle);
                            licensePlateSet.Add(vehicle.LicensePlate); 
                        }
                    }
                }
            }
            //await _context.Trips.AddRangeAsync(validEntries);
            //await _context.SaveChangesAsync();
            return (validEntries, invalidEntries);
        }
    }
}

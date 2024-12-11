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
        public bool checkFile(string path)
        {
            string extension = Path.GetExtension(path);
            return extension == ".xls" || extension == ".xlsx";
        }
        private bool IsValidTrip(TripImportDTO trip)
        {
            if (string.IsNullOrEmpty(trip.Name) ||
                trip.StartTime == default ||
                trip.Price <= 0 || trip.Price == null ||
                string.IsNullOrEmpty(trip.PointStart) ||
                string.IsNullOrEmpty(trip.PointEnd) ||
                string.IsNullOrEmpty(trip.LicensePlate))
            {
                return false;
            }
            return true;
        }
        private async Task<bool> checkTypeVehicle(int? typeVehicle)
        {
            var typeOfVehicel = await _context.VehicleTypes.FirstOrDefaultAsync(x => x.Id == typeVehicle);
            if (typeOfVehicel == null)
            {
                return false;
            }
            return true;
        }
        private bool IsValidVehicel(VehicleImportDTO vehicle)
        {
            if (vehicle.NumberSeat <= 0 ||
                vehicle.VehicleTypeId == null ||
                string.IsNullOrEmpty(vehicle.LicensePlate))
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
            var existingLicensePlates = await _context.Vehicles.Where(x => x.VehicleTypeId == typeOfTrip).Select(v => v.LicensePlate).ToListAsync();
            int rowNumber = 1;
            var licensePlateSet = new HashSet<string>(existingLicensePlates);
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;
                using (var workbook = new XLWorkbook(stream))
                {
                    var tripSheets = workbook.Worksheet("TripInterProvincial");
                    foreach (var row in tripSheets.RowsUsed().Skip(1))
                    {
                        rowNumber++;
                        List<string> error = new List<string>();
                      
                        var trip = new TripImportDTO
                        {
                            Name = row.Cell(1).GetValue<string>(),
                            Description = row.Cell(3).GetValue<string>(),
                            PointStart = row.Cell(5).GetValue<string>(),
                            PointEnd = row.Cell(6).GetValue<string>(),
                            LicensePlate = row.Cell(7).GetValue<string>(),
                            TypeOfTrip = Constant.CHUYEN_DI_LIEN_TINH,
                            Status = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = staffId,
                        };
                        if (!TimeSpan.TryParse(row.Cell(2).GetString(), out var parsedStartTime))
                        {
                            trip.ErrorMessages.Add("Invalid StartTime format in row : " + rowNumber );
                        }
                        else
                        {
                            trip.StartTime = parsedStartTime;
                        }
                        if (!int.TryParse(row.Cell(4).GetString(), out var parsedPrice) || parsedPrice <= 0)
                        {
                            trip.ErrorMessages.Add("Invalid or negative Price in row: " + rowNumber );
                        }
                        else
                        {
                            trip.Price = parsedPrice;
                        }
                        if (!licensePlateSet.Contains(trip.LicensePlate))
                        {
                            trip.ErrorMessages.Add($"License plate '{trip.LicensePlate}' does not exist or vehicle not type interprovincial in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.Name))
                        {
                            trip.ErrorMessages.Add("Name is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.PointEnd))
                        {
                            trip.ErrorMessages.Add("Point End is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.PointStart))
                        {
                            trip.ErrorMessages.Add("Point End is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.LicensePlate))
                        {
                            trip.ErrorMessages.Add("License Plate is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.Description))
                        {
                            trip.ErrorMessages.Add("Description is empty in row: " + rowNumber);
                        }
                        for (int col = 8; col <= tripSheets?.LastColumnUsed()?.ColumnNumber(); col += 2)
                        {
                            var pointDetail = row.Cell(col).GetValue<string>();
                            var pointEnd = row.Cell(col + 3).GetValue<string>();
                            if (TimeSpan.TryParse(row.Cell(col + 1).GetString(), out var pointTimeDetails))
                            {

                                if (!string.IsNullOrEmpty(pointDetail) && !string.IsNullOrEmpty(pointEnd))
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
                                trip.ErrorMessages.Add("Incorrect format time in row: " + rowNumber);
                            }

                        }
                        if (trip.PointStartDetail.Count == 0 || trip.PointEndDetail.Count == 0)
                        {
                            trip.ErrorMessages.Add("PointStartDetail or PointEndDetail are missing in row: " + rowNumber);
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
        public async Task<(List<TripConvenientDTO> validEntries, List<TripConvenientDTO> invalidEntries)> ImportTripConvenience(IFormFile excelFile, int staffId, int typeOfTrip)
        {
            string path = Path.GetFileName(excelFile.FileName);
            if (!checkFile(path))
            {
                throw new Exception("File không hợp lệ");
            }
            var validEntries = new List<TripConvenientDTO>();
            var invalidEntries = new List<TripConvenientDTO>();
            var existingLicensePlates = await _context.Vehicles.Where(x => x.VehicleTypeId == typeOfTrip).Select(v => v.LicensePlate).ToListAsync();
            int rowNumber = 1;
            var licensePlateSet = new HashSet<string>(existingLicensePlates);
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;
                using (var workbook = new XLWorkbook(stream))
                {
                    var tripSheets = workbook.Worksheet("TripConvenience");
                    foreach (var row in tripSheets.RowsUsed().Skip(1))
                    {
                        rowNumber++;
                        List<string> error = new List<string>();
                        var typeOfTripCell = row.Cell(4).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(typeOfTripCell) || (typeOfTripCell != "2" && typeOfTripCell != "3"))
                        {
                            error.Add($"Invalid or empty type Of Trip (only 2 or 3 allowed) in row: {rowNumber}");
                        }

                        var priceCell = row.Cell(5).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(priceCell) || !int.TryParse(priceCell, out int price))
                        {
                            error.Add($"Invalid or empty price in row: {rowNumber}");
                        }
                        var trip = new TripConvenientDTO
                        {
                            Name = row.Cell(1).GetValue<string>(),
                            Description = row.Cell(3).GetValue<string>(),
                            PointStart = row.Cell(6).GetValue<string>(),
                            PointEnd = row.Cell(7).GetValue<string>(),
                            LicensePlate = row.Cell(8).GetValue<string>(),
                            Status = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = staffId,
                        };
                        if (string.IsNullOrWhiteSpace(typeOfTripCell) || !int.TryParse(typeOfTripCell, out int typeOfTripEntries) || (typeOfTripEntries != 2 && typeOfTripEntries != 3))
                        {
                            error.Add($"Invalid or empty type Of Trip (only 2 or 3 allowed) in row: {rowNumber}");
                        }
                        else
                        {
                            trip.TypeOfTrip = typeOfTripEntries;
                        }
                        if (!TimeSpan.TryParse(row.Cell(2).GetString(), out var parsedStartTime))
                        {
                            trip.ErrorMessages.Add("Invalid StartTime format in row : " + rowNumber);
                        }
                        else
                        {
                            trip.StartTime = parsedStartTime;
                        }
                        if (!int.TryParse(row.Cell(5).GetString(), out var parsedPrice) || parsedPrice <= 0 || parsedPrice.ToString() == "null")
                        {
                            trip.ErrorMessages.Add("Invalid or negative price in row: " + rowNumber);
                        }
                        else
                        {
                            trip.Price = parsedPrice;
                        }
                        if (!licensePlateSet.Contains(trip.LicensePlate))
                        {
                            trip.ErrorMessages.Add($"License plate '{trip.LicensePlate}' does not exist or vehicle not type convenience in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.Name))
                        {
                            trip.ErrorMessages.Add("Name is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.PointEnd))
                        {
                            trip.ErrorMessages.Add("Point End is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.PointStart))
                        {
                            trip.ErrorMessages.Add("Point End is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.LicensePlate))
                        {
                            trip.ErrorMessages.Add("License Plate is empty in row: " + rowNumber);
                        }
                        if (string.IsNullOrEmpty(trip.Description))
                        {
                            trip.ErrorMessages.Add("Description is empty in row: " + rowNumber);
                        }
                        if (trip.TypeOfTrip.ToString() == "null" || (trip.TypeOfTrip != 2 && trip.TypeOfTrip != 3))
                        {
                            trip.ErrorMessages.Add($"Invalid or empty type Of Trip (only 2 or 3 allowed) in row: {rowNumber}");
                        }
                        if (trip.ErrorMessages.Count == 0 && error.Count == 0)
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

            if (excelFile == null)
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
            var errors = new List<string>();
            bool flag = false;
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {

                    var tripSheet = workbook.Worksheet("Vehicle");
                    int rowNumber = 1; // Bắt đầu từ dòng tiêu đề
                    foreach (var row in tripSheet.Rows().Skip(1))
                    {
                        rowNumber++;
                        var userNameVehicleOwner = row.Cell(5).GetValue<string>();
                        var vehicleOwnerId = await _context.Users.Where(x => x.Username == userNameVehicleOwner).Select(x => x.Id).FirstOrDefaultAsync();
                        var driverUserName = row.Cell(6).GetValue<string>();
                        var driverId = await _context.Drivers.Where(x => x.UserName == driverUserName).Select(x => x.Id).FirstOrDefaultAsync();
                        errors = new List<string>(); // Đặt lại danh sách lỗi cho mỗi hàng

                        if (row.Cells().All(c => c.IsEmpty()) && errors.Count == 0) 
                        {
                            break;
                        }

                        if (row.IsEmpty() )
                        {
                            errors.Add($"Row {rowNumber} is empty.");
                            invalidEntries.Add(new VehicleImportDTO
                            {
                                Errors = errors
                            });
                            continue; // Bỏ qua xử lý thêm
                        }

                        // Tiến hành kiểm tra từng cột
                        var numberSeatCell = row.Cell(1).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(numberSeatCell) || !int.TryParse(numberSeatCell, out int numberSeat))
                        {
                            errors.Add($"Invalid or empty Number Seat in row: {rowNumber}");
                        }
                        var vehicleTypeIdCell = row.Cell(2).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(vehicleTypeIdCell) || !int.TryParse(vehicleTypeIdCell, out int vehicleTypeId))
                        {
                            errors.Add($"Invalid or empty Vehicle Type ID in row: {rowNumber}");
                        }
                        var vehicleOwner = row.Cell(5).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(vehicleOwner))
                        {
                            errors.Add($"Invalid or empty Vehicle Owner in row: {rowNumber}");
                        }
                        var licensePlateCell = row.Cell(3).GetValue<string>();
                        if (string.IsNullOrWhiteSpace(licensePlateCell))
                        {
                            errors.Add($"Invalid or empty License Plate in row: {rowNumber}");
                        }
                        // Thêm lỗi nếu có
                        if (errors.Count > 0)
                        {
                            invalidEntries.Add(new VehicleImportDTO
                            {
                                Errors = errors
                            });
                            continue; // Bỏ qua xử lý tiếp theo nếu có lỗi
                        }


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
                        //if (row.Cell(1).IsEmpty() || !int.TryParse(row.Cell(1).GetValue<string>(), out int numberSeat))
                        //{
                        //    errors.Add($"Invalid or empty Number Seat in row: {row.RowNumber()}");
                        //}
                        //else
                        //{
                        //    vehicle.NumberSeat = numberSeat;
                        //}
                        if (string.IsNullOrEmpty(vehicle.NumberSeat.ToString()))
                        {
                            errors.Add("Invalid Number Seat in row: " + rowNumber);
                        }
                        if (driverId <= 0 && !string.IsNullOrWhiteSpace(driverUserName))
                        {
                            errors.Add("Invalid Driver Name in row :" + rowNumber);
                        }
                        if (licensePlateSet.Contains(vehicle.LicensePlate))
                        {
                            errors.Add("Duplicate license plate in row :" + rowNumber);
                        }
                        if (!Regex.IsMatch(vehicle.LicensePlate, @"^\d{2}[A-Z]-\d{5}$"))
                        {
                            errors.Add($"Invalid License Plate format '{vehicle.LicensePlate}' in row: {rowNumber}");
                        }
                        if (licensePlateTracker.ContainsKey(vehicle.LicensePlate))
                        {
                            errors.Add($"Duplicate License Plate '{vehicle.LicensePlate}' in row: {rowNumber}");
                        }

                        // Kiểm tra VehicleOwner
                        if (!vehicleOwnerSet.Contains(userNameVehicleOwner))
                        {
                            errors.Add("Invalid VehicleOwner in row :" + rowNumber);
                        }
                        if (!await checkTypeVehicle(vehicle.VehicleTypeId))
                        {
                            errors.Add("Invalid VehicleTypeId  in row: " + rowNumber);
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

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.TripDTOs;
using MyAPI.Models;
using System.Globalization;

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
        private bool IsValidVehicel(Vehicle vehicel)
        {
            if (vehicel.NumberSeat <= 0 ||
                vehicel.VehicleTypeId == null ||
                string.IsNullOrEmpty(vehicel.LicensePlate))
            {
                return false;
            }
            return true;
        }
        public async Task<(List<TripImportDTO> validEntries, List<TripImportDTO> invalidEntries)> ImportTrip(IFormFile excelFile, int staffId)
        {
            string path = Path.GetFileName(excelFile.FileName); 
            if (!checkFile(path)) 
            {
                throw new Exception("File không hợp lệ"); 
            }
            var validEntries = new List<TripImportDTO>(); 
            var invalidEntries = new List<TripImportDTO>(); 
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
                            Status = true,
                            CreatedAt = DateTime.Now, CreatedBy = staffId,
                        }; 
                        if (!TimeSpan.TryParse(row.Cell(2).GetString(), out var parsedStartTime)) 
                        { 
                            invalidEntries.Add(trip);
                            continue;
                        } 
                        trip.StartTime = parsedStartTime;
                        if (!int.TryParse(row.Cell(4).GetString(), out var parsedPrice) || parsedPrice <= 0) 
                        { 
                            invalidEntries.Add(trip); 
                            continue;
                        } 
                        trip.Price = parsedPrice;
                        for (int col = 8; col <= tripSheets?.LastColumnUsed()?.ColumnNumber(); col += 2) 
                        {
                            var pointDetail = row.Cell(col).GetValue<string>();
                            var pointEnd = row.Cell(col + 3).GetValue<string>();
                            if (TimeSpan.TryParse(row.Cell(col + 1).GetString(), out var pointTimeDetails))
                            {
                                if (!string.IsNullOrEmpty(pointDetail))
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
                        }                 
                        if (IsValidTrip(trip) && (trip.PointEndDetail.Count != 0 &&  trip.PointStartDetail.Count != 0)) 
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
            //await _context.Trips.AddRangeAsync(validEntries);
            //await _context.SaveChangesAsync();
            return (validEntries, invalidEntries);
        }
        public async Task ImportTripDetailsByTripId(IFormFile excelFileTripDetails, int staffId, int tripId)
        {
            string path = Path.GetFileName(excelFileTripDetails.FileName);
            if (checkFile(path))
            {
                using var stream = new MemoryStream();
                await excelFileTripDetails.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var tripSheets = workbook.Worksheet("TripDetail");
                var tripDetails = new List<TripDetail>();
                foreach (var row in tripSheets.RowsUsed().Skip(1))
                {
                    var tripDetail = new TripDetail
                    {
                        PointStartDetails = row.Cell(1).GetValue<string>(),
                        PointEndDetails = row.Cell(2).GetValue<string>(),
                        TimeStartDetils = row.Cell(3).GetValue<TimeSpan>(),
                        TimeEndDetails = row.Cell(4).GetValue<TimeSpan>(),
                        TripId = tripId,
                        Status = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = staffId,
                        UpdateAt = null,
                        UpdateBy = null,
                    };
                    tripDetails.Add(tripDetail);
                }
                await _context.TripDetails.AddRangeAsync(tripDetails);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("File không hợp lệ");
            }
        }


        public async Task<(List<Vehicle> validEntries, List<Vehicle> invalidEntries)> ImportVehicel(IFormFile excelFile, int staffId)
        {
            string path = Path.GetFileName(excelFile.FileName);
            if (!checkFile(path))
            {
                throw new Exception("File không hợp lệ");
            }
            var validEntries = new List<Vehicle>();
            var invalidEntries = new List<Vehicle>();
            var existingLicensePlates = await _context.Vehicles.Select(v => v.LicensePlate).ToListAsync();

            var licensePlateSet = new HashSet<string>(existingLicensePlates);

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var tripSheets = workbook.Worksheet("Vehicle");
                    foreach (var row in tripSheets.RowsUsed().Skip(1))
                    {
                        var vehicle = new Vehicle
                        {
                            NumberSeat = row.Cell(1).GetValue<Int32>(),
                            VehicleTypeId = row.Cell(2).GetValue<Int32>(),
                            LicensePlate = row.Cell(3).GetValue<string>(),
                            Description = row.Cell(4).GetValue<string>(),
                            Status = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = staffId,
                            UpdateAt = null,
                            UpdateBy = null,
                        };

                        if (licensePlateSet.Contains(vehicle.LicensePlate))
                        {
                            invalidEntries.Add(vehicle);
                        }
                        else if (IsValidVehicel(vehicle) && await checkTypeVehicle(vehicle.VehicleTypeId))
                        {
                            validEntries.Add(vehicle);
                            licensePlateSet.Add(vehicle.LicensePlate); // Thêm vào danh sách biển số đã kiểm tra
                        }
                        else
                        {
                            invalidEntries.Add(vehicle);
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

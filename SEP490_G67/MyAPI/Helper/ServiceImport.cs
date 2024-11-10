﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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
        private bool IsValidTrip(Trip trip)
        {
            if (string.IsNullOrEmpty(trip.Name) ||
                trip.StartTime == default ||
                trip.Price <= 0 || trip.Price == null || 
                string.IsNullOrEmpty(trip.PointStart) ||
                string.IsNullOrEmpty(trip.PointEnd))
            {
                return false;
            }
            return true;
        }
        public async Task<(List<Trip> validEntries, List<Trip> invalidEntries)> ImportTrip(IFormFile excelFile, int staffId)
        {
            string path = Path.GetFileName(excelFile.FileName); 
            if (!checkFile(path)) 
            {
                throw new Exception("File không hợp lệ"); 
            }
            var validEntries = new List<Trip>(); 
            var invalidEntries = new List<Trip>(); 
            using (var stream = new MemoryStream()) 
            {
                await excelFile.CopyToAsync(stream); 
                using (var workbook = new XLWorkbook(stream)) 
                { var tripSheets = workbook.Worksheet("Trip"); 
                    foreach (var row in tripSheets.RowsUsed().Skip(1)) 
                    { 
                        var trip = new Trip 
                        { 
                            Name = row.Cell(1).GetValue<string>(), 
                            Description = row.Cell(3).GetValue<string>(), 
                            PointStart = row.Cell(5).GetValue<string>(), 
                            PointEnd = row.Cell(6).GetValue<string>(), 
                            Status = true, CreatedAt = DateTime.Now, 
                            CreatedBy = staffId, 
                            UpdateAt = null, 
                            UpdateBy = null, 
                        };
                        var startTimeCellValue = row.Cell(2).GetString();
                        if (TimeSpan.TryParse(startTimeCellValue, out TimeSpan parsedStartTime))
                        {
                            trip.StartTime = parsedStartTime;
                        }
                        else
                        {
                            invalidEntries.Add(trip);
                            continue;
                        }
                        var priceCellValue = row.Cell(4).GetString();
                        if (int.TryParse(priceCellValue, out int parsedPrice) && parsedPrice > 0)
                        {
                            trip.Price = parsedPrice;
                        }
                        else
                        {
                            invalidEntries.Add(trip);
                            continue;
                        }
                        if (IsValidTrip(trip))
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
    }
}
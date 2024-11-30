using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.RevenueDTOs;
using MyAPI.Repositories.Impls;
using OfficeOpenXml;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly RevenueRepository _revenueRepository;
        public RevenueController(RevenueRepository revenueRepository)
        {
            _revenueRepository = revenueRepository;
        }
        [HttpGet("getRevenue/{timeStart}/{timeEnd}")]
        public async Task<IActionResult> getRevenue(DateTime timeStart, DateTime timeEnd, int? vehicleId, int? vehicleOwner)
        {
            try
            {
                var result = await _revenueRepository.RevenueStatistic(timeStart, timeEnd, vehicleId, vehicleOwner);
                return Ok(result);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("exportRevenue")]
        public async Task<IActionResult> exportRevenue([FromBody] RevenueDTO data)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    
                    var ticketSheet = package.Workbook.Worksheets.Add("Revenue Tickets");
                    ticketSheet.Cells[1, 1].Value = "Liscense Vehicle";
                    ticketSheet.Cells[1, 2].Value = "Type Of Ticket";
                    ticketSheet.Cells[1, 3].Value = "Type Of Payment";
                    ticketSheet.Cells[1, 4].Value = "Price Promotion";
                    ticketSheet.Cells[1, 5].Value = "Created At";

                    int row = 2;
                    
                    foreach (var ticket in data.revenueTicketDTOs[0].listTicket)
                    {
                        ticketSheet.Cells[row, 1].Value = ticket.LiscenseVehicle;
                        ticketSheet.Cells[row, 2].Value = ticket.TypeOfTicket;
                        ticketSheet.Cells[row, 3].Value = ticket.TypeOfPayment;
                        ticketSheet.Cells[row, 4].Value = ticket.PricePromotion ?? 0;
                        ticketSheet.Cells[row, 5].Value = ticket.CreatedAt;
                        ticketSheet.Cells[row, 5].Style.Numberformat.Format = "dd/MM/yyyy";
                        row++;
                    }
                   
                    int ticketEndRow = row - 1;
                    if (row < 3)
                    {
                        ticketSheet.Cells[row, 4].Formula = "0";
                        ticketSheet.Cells[row, 1].Style.Font.Bold = true;
                        ticketSheet.Cells[row, 1].Value = "Total Revenue Tickets";
                    }
                    else
                    {
                        ticketSheet.Cells[row, 4].Formula = $"SUM(D2:D{ticketEndRow})";
                        ticketSheet.Cells[row, 4].Style.Font.Bold = true;
                        ticketSheet.Cells[2, 4, row - 1, 4].Style.Numberformat.Format = "#,##0";
                        ticketSheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                        ticketSheet.Cells[row, 1].Value = "Total Revenue Tickets";
                    }
                  

                   
                    var driverSheet = package.Workbook.Worksheets.Add("Payment Rent Drivers");
                    driverSheet.Cells[1, 1].Value = "Driver Name";
                    driverSheet.Cells[1, 2].Value = "License Vehicle";
                    driverSheet.Cells[1, 3].Value = "Price";
                    driverSheet.Cells[1, 4].Value = "Created At";
                    driverSheet.Cells[row, 4].Style.Numberformat.Format = "dd/MM/yyyy";

                    row = 2;
                    foreach (var driver in data.totalPayementRentDrivers[0].PaymentRentDriverDTOs)
                    {
                        driverSheet.Cells[row, 1].Value = driver.DriverName;
                        driverSheet.Cells[row, 2].Value = driver.LicenseVehicle;
                        driverSheet.Cells[row, 3].Value = driver.Price ?? 0;
                        driverSheet.Cells[row, 4].Value = driver.CreatedAt;
                        row++;
                    }

                   
                    int driverEndRow = row - 1;
                    if(row < 3)
                    {
                        driverSheet.Cells[row, 1].Value = "Total Rent Drivers";
                        driverSheet.Cells[row, 3].Formula = "0";
                        driverSheet.Cells[row, 3].Style.Font.Bold = true;
                    }
                    else
                    {
                        driverSheet.Cells[row, 1].Value = "Total Rent Drivers";
                        driverSheet.Cells[row, 3].Formula = $"SUM(C2:C{driverEndRow})";
                        driverSheet.Cells[2, 3, row - 1, 3].Style.Numberformat.Format = "#,##0";
                        driverSheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                        driverSheet.Cells[row, 3].Style.Font.Bold = true;
                    }
                    
                   
                    var carSheet = package.Workbook.Worksheets.Add("Total Rent Car");
                    carSheet.Cells[1, 1].Value = "Driver Name";
                    carSheet.Cells[1, 2].Value = "License Vehicle";
                    carSheet.Cells[1, 3].Value = "Price";
                    carSheet.Cells[1, 4].Value = "Create At";
                    carSheet.Cells[1, 5].Value = "Car Owner ";

                    row = 2;
                    foreach (var car in data.totalPaymentRentVehicleDTOs[0].PaymentRentVehicelDTOs)
                    {
                        carSheet.Cells[row, 1].Value = car.DriverName;
                        carSheet.Cells[row, 2].Value = car.LicenseVehicle;
                        carSheet.Cells[row, 3].Value = car.Price ?? 0;
                        carSheet.Cells[row, 4].Value = car.CreatedAt;
                        carSheet.Cells[row, 4].Style.Numberformat.Format = "dd/MM/yyyy";
                        carSheet.Cells[row, 5].Value = car.CarOwner;
                        row++;
                    }
                    int vehicleEndRow = row - 1;

                    if (row < 3)
                    {
                        carSheet.Cells[row, 1].Value = "Total Rent Drivers";
                        carSheet.Cells[row, 3].Formula = "0";
                        carSheet.Cells[row, 3].Style.Font.Bold = true;
                    }
                    else
                    {
                        carSheet.Cells[row, 1].Value = "Total Rent Drivers";
                        carSheet.Cells[row, 3].Formula = $"SUM(C2:C{vehicleEndRow})";
                        carSheet.Cells[2, 3, row - 1, 3].Style.Numberformat.Format = "#,##0";
                        carSheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                        carSheet.Cells[row, 3].Style.Font.Bold = true;
                    }
                    

                   
                    var lossSheet = package.Workbook.Worksheets.Add("Total Loss Cost");
                    lossSheet.Cells[1, 1].Value = "License Plate";
                    lossSheet.Cells[1, 2].Value = "Loss Cost Type";
                    lossSheet.Cells[1, 3].Value = "Price";
                    lossSheet.Cells[1, 4].Value = "Description";
                    lossSheet.Cells[1, 5].Value = "Car Owner Id";
                    lossSheet.Cells[1, 6].Value = "Date Incurred";
                    row = 2;
                    foreach (var losstCost in data.totalLossCosts[0].listLossCostVehicle)
                    {
                        lossSheet.Cells[row, 1].Value = losstCost.LicensePlate;
                        lossSheet.Cells[row, 2].Value = losstCost.LossCostType;
                        lossSheet.Cells[row, 3].Value = losstCost.Price ?? 0;
                        lossSheet.Cells[row, 4].Value = losstCost.Description;
                        lossSheet.Cells[row, 5].Value = losstCost.VehicleOwner;
                        lossSheet.Cells[row, 6].Value = losstCost.DateIncurred;
                        lossSheet.Cells[row, 6].Style.Numberformat.Format = "dd/MM/yyyy";
                        row++;
                    }
                    int lossSheetEndRow = row - 1;
                    if (row < 3)
                    {
                        lossSheet.Cells[row, 1].Value = "Total Rent Drivers";
                        lossSheet.Cells[row, 3].Formula = "0";
                        lossSheet.Cells[row, 3].Style.Font.Bold = true;
                    }
                    else
                    {
                        lossSheet.Cells[row, 1].Value = "Total Rent Drivers";
                        lossSheet.Cells[row, 3].Formula = $"SUM(C2:C{lossSheetEndRow})";
                        lossSheet.Cells[2, 3, row - 1, 3].Style.Numberformat.Format = "#,##0";
                        lossSheet.Cells[row, 3].Style.Numberformat.Format = "#,##0"; 
                        lossSheet.Cells[row, 3].Style.Font.Bold = true;
                    }
                    
                   
                    var summarySheet = package.Workbook.Worksheets.Add("Total Revenue");
                    summarySheet.Cells[1, 1].Value = "Description";
                    summarySheet.Cells[1, 2].Value = "Amount";

                   
                    summarySheet.Cells[2, 1].Value = "Total from Revenue Tickets";
                    summarySheet.Cells[2, 2].Formula = $"='Revenue Tickets'!D{ticketEndRow + 1}";

                    summarySheet.Cells[3, 1].Value = "Total from Rent Drivers";
                    summarySheet.Cells[3, 2].Formula = $"='Payment Rent Drivers'!C{driverEndRow + 1}";

                    summarySheet.Cells[4, 1].Value = "Total Loss Cost Vehicle";
                    summarySheet.Cells[4, 2].Formula = $"='Total Loss Cost'!C{lossSheetEndRow + 1}";

                    summarySheet.Cells[5, 1].Value = "Total from Rent Vehicle";
                    summarySheet.Cells[5, 2].Formula = $"='Total Rent Car'!C{vehicleEndRow + 1}";
                    
                    summarySheet.Cells[6, 1].Value = "Grand Total";
                    summarySheet.Cells[6, 2].Formula = $"=(B2 - B3 - B4 + B5)";
                    summarySheet.Cells[6, 2].Style.Font.Bold = true;
                    summarySheet.Cells[2, 2, 6, 2].Style.Numberformat.Format = "#,##0";
                    ticketSheet.Cells.AutoFitColumns();

                    driverSheet.Cells.AutoFitColumns();
                    carSheet.Cells.AutoFitColumns();
                    lossSheet.Cells.AutoFitColumns();
                    summarySheet.Cells.AutoFitColumns();
                 
                    var stream = new MemoryStream(package.GetAsByteArray());
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileName = "Revenue_Report.xlsx";

                    return File(stream, contentType, fileName);
                }
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
       
    }
}

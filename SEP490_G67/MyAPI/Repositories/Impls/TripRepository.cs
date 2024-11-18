using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.TripDTOs;
using MyAPI.DTOs.VehicleDTOs;
using MyAPI.DTOs.VehicleTripDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using OfficeOpenXml;

namespace MyAPI.Repositories.Impls
{
    public class TripRepository : GenericRepository<Trip>, ITripRepository
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;

        public TripRepository(SEP490_G67Context _context, IMapper mapper, IHttpContextAccessor httpContextAccessor, GetInforFromToken tokenHelper) : base(_context)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
        }

        public async Task<List<TripDTO>> GetListTrip()
        {
            try
            {
                var tripList = await _context.Trips.ToListAsync();
                var mapperTrip = _mapper.Map<List<TripDTO>>(tripList);
                return mapperTrip;
            }
            catch (Exception ex)
            {
                throw new Exception("GetListTrip: " + ex.Message);
            }
        }
        public async Task<List<TripVehicleDTO>> SreachTrip(string startPoint, string endPoint, string? time)
        {
            try
            {
                var timeSpan = TimeSpan.Parse(time);
                var searchTrip = await (from t in _context.Trips
                                        join tv in _context.VehicleTrips
                                        on t.Id equals tv.TripId
                                        join v in _context.Vehicles
                                        on tv.VehicleId equals v.Id
                                        join u in _context.Users on v.VehicleOwner equals u.Id
                                        where t.PointStart.Contains(startPoint) &&
                                            t.PointEnd.Contains(endPoint) &&
                                            t.StartTime >= timeSpan &&
                                            t.TypeOfTrip == Constant.CHUYEN_DI_LIEN_TINH &&
                                            t.Status == true &&
                                            v.Status == true
                                        group new { t, v, u } by new
                                        {
                                            t.Id,
                                            u.FullName,
                                            t.Description,
                                            t.PointStart,
                                            t.PointEnd,
                                            t.StartTime
                                        } into tripGroup
                                        select new TripVehicleDTO
                                        {
                                            Id = tripGroup.Key.Id,
                                            FullName = tripGroup.Key.FullName,
                                            Description = tripGroup.Key.Description,
                                            PointStart = tripGroup.Key.PointStart,
                                            PointEnd = tripGroup.Key.PointEnd,
                                            StartTime = tripGroup.Key.StartTime,
                                            listVehicle = tripGroup.Select(g => new VehicleDTO
                                            {
                                                LicensePlate = g.v.LicensePlate,
                                                NumberSeat = g.v.NumberSeat,
                                                VehicleTypeId = g.v.VehicleTypeId,
                                                Price = g.t.Price,

                                            }).OrderByDescending(v => v.LicensePlate).ToList()
                                        }).ToListAsync();
                var searchTripMapper = _mapper.Map<List<TripVehicleDTO>>(searchTrip);
                return searchTripMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("SreachTrip: " + ex.Message);
            }
        }
        public async Task AddTrip(TripDTO trip, int? vehicleId, int userId)
        {
            try
            {
                Trip addTrip = new Trip
                {
                    Name = trip.Name,
                    Description = trip.Description,
                    PointStart = trip.PointStart,
                    PointEnd = trip.PointEnd,
                    StartTime = trip.StartTime,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    Price = trip.Price,
                    UpdateAt = null,
                    UpdateBy = null,
                    Status = trip.Status,
                };
                _context.Add(addTrip);
                await _context.SaveChangesAsync();
                List<TripDetail> td = new List<TripDetail>();
                var pointEndDetails = trip.PointEndDetail.First();
                var pointEndDetail = pointEndDetails.Key;
                var timeEndDetail = pointEndDetails.Value;
                foreach(var pointStart in trip.PointStartDetail)
                {
                    var PointStartDetails = pointStart.Key;
                    var TimeStartDetails = pointStart.Value;
                    var tripDetail = new TripDetail
                    {
                        PointStartDetails = PointStartDetails,
                        TimeStartDetils = TimeStartDetails,
                        PointEndDetails = pointEndDetail,
                        TimeEndDetails = timeEndDetail,
                        TripId = addTrip.Id,
                        Status = true
                    };
                    td.Add(tripDetail);
                }
                await _context.AddRangeAsync(td);
                await _context.SaveChangesAsync();
                var vechicleById = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
                if (vechicleById != null)
                {
                    VehicleTripDTO vehicleTripDTO = new VehicleTripDTO
                    {
                        TripId = addTrip.Id,
                        VehicleId = vechicleById.Id,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId,
                        UpdateAt = null,
                        UpdateBy = null
                    };
                    var vehicleTripMapper = _mapper.Map<VehicleTrip>(vehicleTripDTO);
                    _context.VehicleTrips.Add(vehicleTripMapper);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("addTrips: " + ex.Message, ex);
            }
        }
        public async Task UpdateTripById(int tripId, TripDTO trip, int userId)
        {
            try
            {
                var tripById = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
                if (tripById != null)
                {
                    tripById.Name = trip.Name;
                    tripById.Description = trip.Description;
                    tripById.Price = trip.Price;
                    tripById.PointStart = trip.PointStart;
                    tripById.PointEnd = trip.PointEnd;
                    tripById.StartTime = trip.StartTime;
                    tripById.UpdateAt = DateTime.Now;
                    tripById.UpdateBy = userId;
                    tripById.Status = trip.Status;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateTripById: " + ex.Message);
            }

        }
        public async Task updateStatusTrip(int id, int userId)
        {
            try
            {
                var tripById = await _context.Trips.FirstOrDefaultAsync(x => x.Id == id);
                if (tripById != null)
                {
                    tripById.Status = (tripById.Status == true) ? false : true;
                    tripById.UpdateBy = userId;
                    tripById.UpdateAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("updateStatusTrip: " + ex.Message);
            }
        }
        public async Task confirmAddValidEntryImport(List<TripImportDTO> validEntries)
        {
            try
            {
                List<VehicleTrip> vt = new List<VehicleTrip>();
                List<TripDetail> td = new List<TripDetail>();

                var tripMapper = _mapper.Map<List<Trip>>(validEntries);
                _context.Trips.AddRange(tripMapper);
                await _context.SaveChangesAsync();

                for (int i = 0; i < tripMapper.Count; i++)
                {
                    var pointEndDetail = validEntries[i].PointEndDetail.First();
                    var PointEndDetails = pointEndDetail.Key;
                    var TimeEndDetails = pointEndDetail.Value;
                    foreach (var pointStart in validEntries[i].PointStartDetail)
                    {

                        var PointStartDetails = pointStart.Key; 
                        var TimeStartDetails = pointStart.Value;
                        var tripDetail = new TripDetail
                        {
                            PointStartDetails = PointStartDetails,
                            TimeStartDetils = TimeStartDetails,
                            PointEndDetails = PointEndDetails,
                            TimeEndDetails = TimeEndDetails,
                            TripId = tripMapper[i].Id,
                            Status = true
                        }; 
                        td.Add(tripDetail);
                    }
                    string licensePlate = validEntries[i].LicensePlate;

                    var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
                    // assgin vehicle
                    if (vehicle != null)
                    {
                        int vehicleId = vehicle.Id;
                        var vehicleTrip = new VehicleTrip
                        {
                            TripId = tripMapper[i].Id,
                            VehicleId = vehicleId,  
                            CreatedAt = tripMapper[i].CreatedAt,
                            CreatedBy = tripMapper[i].CreatedBy,
                        };
                        vt.Add(vehicleTrip);
                    }

                }
                await _context.VehicleTrips.AddRangeAsync(vt);
                //await _context.TripDetails.AddRangeAsync(td);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error during import: " + ex.Message);
            }
        }

        public async Task<List<TripDTO>> getListTripNotVehicle()
        {
            try
            {
                var result = await (from t in _context.Trips
                                    join vt in _context.VehicleTrips on t.Id equals vt.TripId into tripVehicle
                                    from vt in tripVehicle.DefaultIfEmpty()
                                    join v in _context.Vehicles on vt.VehicleId equals v.Id into vehicleGroup
                                    from v in vehicleGroup.DefaultIfEmpty()
                                    where vt == null
                                    select t
                             ).ToListAsync();

                var listTripNoVehicleMapper = _mapper.Map<List<TripDTO>>(result);
                return listTripNoVehicleMapper;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<int> GetTicketCount(int tripId)
        {
            try
            {
                var vehicelID = await _context.VehicleTrips.FirstOrDefaultAsync(x => x.TripId == tripId);

                var listTicketByVehicelID = await (from t in _context.Tickets
                                                   where t.VehicleId == vehicelID.VehicleId
                                                   select t.NumberTicket
                                         ).ToListAsync();
                var sum = listTicketByVehicelID.Sum();
                return sum ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public async Task<(List<Trip>, List<string>)> ImportExcel(Stream excelStream)
        {

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            var correctTrips = new List<Trip>();
            var errorAdd = new List<string>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var name = worksheet.Cells[row, 1].Text;
                        var startTimeText = worksheet.Cells[row, 2].Text;
                        var description = worksheet.Cells[row, 3].Text;
                        var priceText = worksheet.Cells[row, 4].Text;
                        var pointStart = worksheet.Cells[row, 5].Text;
                        var pointEnd = worksheet.Cells[row, 6].Text;
                        var statusText = worksheet.Cells[row, 7].Text;
                        var typeOfTripText = worksheet.Cells[row, 8].Text;

                        if (string.IsNullOrEmpty(name))
                        {
                            errorAdd.Add($"Row{row}: Name is required");
                            continue;
                        }

                        if (string.IsNullOrEmpty(pointStart))
                        {
                            errorAdd.Add($"Row{row}: pointStart is required");
                            continue;
                        }

                        if (string.IsNullOrEmpty(pointEnd))
                        {
                            errorAdd.Add($"Row{row}: pointEnd is required");
                            continue;
                        }

                        if (!TimeSpan.TryParse(startTimeText, out TimeSpan startTime))
                        {
                            errorAdd.Add($"Row{row}: Invalid Start Time format. Use hh:mm:ss.");
                            continue;
                        }

                        if (!decimal.TryParse(priceText, out decimal price) || price < 0)
                        {
                            errorAdd.Add($"Row{row}: Price invalid or not negative");
                            continue;
                        }

                        bool? status = statusText.ToLower() switch { "true" => true, "false" => false, _ => (bool?)null };

                        if (!int.TryParse(typeOfTripText, out int typeOfTrip))
                        {
                            errorAdd.Add($"Row{row}: Invalid typeOfTripText");
                            continue;
                        }


                        var tripAdd = new Trip
                        {
                            Name = name,
                            StartTime = startTime,
                            Description = description,
                            Price = price,
                            PointStart = pointStart,
                            PointEnd = pointEnd,
                            Status = status,
                            TypeOfTrip = typeOfTrip,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId,
                            UpdateAt = DateTime.Now,
                            UpdateBy = userId,
                        };

                        correctTrips.Add(tripAdd);

                    }
                    catch (Exception ex)
                    {
                        errorAdd.Add($"Row {row}: {ex.Message}");
                    }
                }
            }

            try
            {
                if (correctTrips.Count > 0)
                {
                    await _context.Trips.AddRangeAsync(correctTrips);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                errorAdd.Add($"Error adding to database: {ex.Message}");
            }

            return (correctTrips, errorAdd);

        }


        public async Task<decimal> SearchVehicleConvenient(string startPoint, string endPoint, int typeOfTrip)
        {
            if (string.IsNullOrEmpty(startPoint) || string.IsNullOrEmpty(endPoint))
                throw new ArgumentException("Start point and end point must not be empty.");

            var price = await _context.Trips
                .Where(t => t.PointStart.Contains(startPoint) && t.PointEnd.Contains(endPoint) && t.TypeOfTrip == typeOfTrip)
                .Select(s => s.Price)
                .FirstOrDefaultAsync();

            if (price == 0 || price == null)
                throw new Exception("Start or end point or typeOfTrip is invalid!");

            return price.Value;
        }

    }
}

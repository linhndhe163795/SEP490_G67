using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.TripDetailsDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class TripDetailsRepository : GenericRepository<TripDetail>, ITripDetailsRepository
    {
        private readonly IMapper _mapper;
        public TripDetailsRepository(SEP490_G67Context context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<List<EndPointTripDetails>> EndPointTripDetailsById(int TripId)
        {
            if (TripId == null)
            {
                throw new ArgumentNullException(nameof(TripId), "Trip ID cannot be null.");
            }

            if (TripId <= 0)
            {
                throw new ArgumentException("Trip ID must be a valid positive integer.");
            }

            try
            {
                var listEndPointTripDetails = await _context.TripDetails
                    .Where(x => x.TripId == TripId && x.Status == true)
                    .ToListAsync();

                if (listEndPointTripDetails == null || !listEndPointTripDetails.Any())
                {
                    throw new KeyNotFoundException("No endpoint trip details found for the specified Trip ID.");
                }

                var listEndPointTripDetailsMapper = _mapper.Map<List<EndPointTripDetails>>(listEndPointTripDetails)
                    .GroupBy(x => new { x.PointEndDetails, x.TimeEndDetails })
                    .Select(g => g.First())
                    .ToList();

                return listEndPointTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("EndPointTripDetailsById: " + ex.Message);
            }
        }


        public async Task<List<StartPointTripDetails>> StartPointTripDetailsById(int TripId)
        {
            if (TripId == null)
            {
                throw new ArgumentNullException(nameof(TripId), "Trip ID cannot be null.");
            }

            if (TripId <= 0)
            {
                throw new ArgumentException("Trip ID must be a valid positive integer.");
            }

            try
            {
                var listStartPointTripDetails = await _context.TripDetails
                    .Where(x => x.TripId == TripId && x.Status == true)
                    .ToListAsync();

                if (listStartPointTripDetails == null || !listStartPointTripDetails.Any())
                {
                    throw new KeyNotFoundException("No start point trip details found for the specified Trip ID.");
                }

                var listStartPointTripDetailsMapper = _mapper.Map<List<StartPointTripDetails>>(listStartPointTripDetails);
                return listStartPointTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("StartPointTripDetailsById: " + ex.Message);
            }
        }


        public async Task<List<TripDetailsDTO>> TripDetailsByTripId(int TripId)
        {


            if (TripId <= 0)
            {
                throw new ArgumentException("Trip ID must be a valid positive integer.");
            }

            try
            {
                var listTripDetails = await _context.TripDetails
                    .Where(x => x.TripId == TripId && x.Status == true)
                    .ToListAsync();

                if (listTripDetails == null || !listTripDetails.Any())
                {
                    throw new KeyNotFoundException("No trip details found for the specified Trip ID.");
                }

                var listTripDetailsMapper = _mapper.Map<List<TripDetailsDTO>>(listTripDetails);
                return listTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("TripDetailsByTripId: " + ex.Message);
            }
        }


        public async Task UpdateTripDetailsById(int tripId, int tripDetailsId, UpdateTripDetails updateTripDetails)
        {
            try
            {
                if (tripId <= 0)
                {
                    throw new Exception("Invalid trip ID.");
                }

                if (tripDetailsId <= 0)
                {
                    throw new Exception("Invalid trip details ID.");
                }

                if (updateTripDetails == null)
                {
                    throw new Exception("Update data is required.");
                }

                if (string.IsNullOrWhiteSpace(updateTripDetails.PointStartDetails))
                {
                    throw new Exception("PointStartDetails is required.");
                }

                if (string.IsNullOrWhiteSpace(updateTripDetails.PointEndDetails))
                {
                    throw new Exception("PointEndDetails is required.");
                }

                if (updateTripDetails.TimeStartDetils >= updateTripDetails.TimeEndDetails)
                {
                    throw new Exception("TimeStartDetils must be earlier than TimeEndDetails.");
                }
                var tripDetail = await _context.TripDetails
                    .FirstOrDefaultAsync(x => x.TripId == tripId && x.Id == tripDetailsId);
                if (tripDetail == null)
                {
                    throw new Exception("Not Found trip Details");
                }
                tripDetail.PointStartDetails = updateTripDetails.PointStartDetails;
                tripDetail.TimeStartDetils = updateTripDetails.TimeStartDetils;
                tripDetail.PointEndDetails = updateTripDetails.PointEndDetails;
                tripDetail.TimeEndDetails = updateTripDetails.TimeEndDetails;
                tripDetail.Status = updateTripDetails.Status;
                _context.TripDetails.Update(tripDetail);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddTripDetailsByTripId(int tripId, TripDetailsDTO tripDetailsDTO, int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(tripDetailsDTO.PointStartDetails))
                {
                    throw new Exception("Point Start Details is not null");
                }
                if (string.IsNullOrEmpty(tripDetailsDTO.PointEndDetails))
                {
                    throw new Exception("Point End Details is not null");
                }
                // Kiểm tra TimeStartDetails và TimeEndDetails có giá trị và hợp lệ
                if (tripDetailsDTO.TimeStartDetils.HasValue && tripDetailsDTO.TimeEndDetails.HasValue)
                {
                    if (tripDetailsDTO.TimeStartDetils.Value >= tripDetailsDTO.TimeEndDetails.Value)
                    {
                        throw new Exception("TimeStartDetils must be less than TimeEndDetails.");
                    }
                }
                var tripDetails = new TripDetail
                {
                    PointEndDetails = tripDetailsDTO.PointEndDetails,
                    TimeStartDetils = tripDetailsDTO.TimeStartDetils,
                    PointStartDetails = tripDetailsDTO.PointStartDetails,
                    TimeEndDetails = tripDetailsDTO.TimeEndDetails,
                    TripId = tripId,
                    Status = tripDetailsDTO.Status,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };
                _context.TripDetails.Add(tripDetails);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
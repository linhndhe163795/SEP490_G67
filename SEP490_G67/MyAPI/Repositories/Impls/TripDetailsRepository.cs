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
            try
            {

                if (TripId <= 0)
                {
                    throw new Exception("Invalid Trip");
                }
                var listEndPointTripDetails = await _context.TripDetails.Where(x => x.TripId == TripId && x.Status == true).ToListAsync();
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
            try
            {
                if (TripId <= 0)
                {
                    throw new Exception("Invalid Trip");
                }
                var listStartPointTripDetails = await _context.TripDetails.Where(x => x.TripId == TripId && x.Status == true).ToListAsync();
                var listStartPointTripDetailsMapper = _mapper.Map<List<StartPointTripDetails>>(listStartPointTripDetails);
                return listStartPointTripDetailsMapper;
            }
            catch (Exception ex)
            {
                throw new Exception("EndPointTripDetailsById: " + ex.Message);
            }
        }

        public async Task<List<TripDetailsDTO>> TripDetailsByTripId(int TripId)
        {
            try
            {
                if (TripId <= 0)
                {
                    throw new Exception("Invalid Trip");
                }
                var listTripDetails = await _context.TripDetails.Where(x => x.TripId == TripId && x.Status == true).ToListAsync();
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
                if(tripDetail == null)
                {
                    throw new Exception("Not Found trip Details");
                }
                tripDetail.PointStartDetails = updateTripDetails.PointStartDetails;
                tripDetail.TimeStartDetils = updateTripDetails.TimeStartDetils;
                tripDetail.PointEndDetails = updateTripDetails.PointEndDetails;
                tripDetail.TimeEndDetails = updateTripDetails.TimeEndDetails;
                _context.TripDetails.Update(tripDetail);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

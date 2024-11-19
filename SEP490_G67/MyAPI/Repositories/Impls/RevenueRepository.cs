using DocumentFormat.OpenXml.VariantTypes;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.DTOs.PaymentRentDriver;
using MyAPI.DTOs.PaymentRentVehicle;
using MyAPI.DTOs.RevenueDTOs;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class RevenueRepository : IRevenueRepository
    {
        private readonly SEP490_G67Context _context;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPaymentRentVehicleRepository _paymentRentVehicleRepository;
        private readonly ILossCostVehicleRepository _lossCostVehicleRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly IHistoryRentDriverRepository _historyRentDriverRepository;
        public RevenueRepository(
            SEP490_G67Context context,
            ITicketRepository ticketRepository,
            IHttpContextAccessor httpContextAccessor,
            GetInforFromToken getInforFromToken,
            IPaymentRentVehicleRepository paymentRentVehicleRepository,
            IHistoryRentDriverRepository historyRentDriverRepository,
            ILossCostVehicleRepository lossCostVehicleRepository
            )
        {
            _context = context;
            _ticketRepository = ticketRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = getInforFromToken;
            _paymentRentVehicleRepository = paymentRentVehicleRepository;
            _lossCostVehicleRepository = lossCostVehicleRepository;
            _historyRentDriverRepository = historyRentDriverRepository;


        }
        public async Task<RevenueDTO> RevenueStatistic(DateTime startTime, DateTime endTime, int? vehicleId, int? vehicleOwner)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);
            var listRenvenueTicket = await _ticketRepository.getRevenueTicket(startTime, endTime, vehicleId, vehicleOwner, userId);
            var listRevenueRentVehicle = await _paymentRentVehicleRepository.getPaymentRentVehicleByDate(startTime, endTime, vehicleId, vehicleOwner, userId);
            var listLossCost = await _lossCostVehicleRepository.GetLossCostVehicleByDate(vehicleId,startTime,endTime, vehicleOwner, userId);
            var listExpenseRentDriver = await _historyRentDriverRepository.GetRentDetailsWithTotalForOwner(startTime, endTime, vehicleId, vehicleOwner);

            var revenue = new RevenueDTO
            {
                revenueTicketDTOs = new List<RevenueTicketDTO> { listRenvenueTicket },
                totalLossCosts = new List<TotalLossCost> { listLossCost},
                totalPayementRentDrivers = new List<TotalPayementRentDriver> { listExpenseRentDriver},
                totalPaymentRentVehicleDTOs = new List<TotalPaymentRentVehicleDTO> { listRevenueRentVehicle }
            };
            return revenue;
        }
    }
}

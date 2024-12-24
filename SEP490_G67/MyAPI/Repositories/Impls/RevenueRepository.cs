using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<RevenueDTO> RevenueStatistic()
        {

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);


            if (userId <= 0)
            {
                throw new Exception("Invalid user ID from token.");
            }

            var listRenvenueTicket = await _ticketRepository.getRevenueTicket(userId);
            var listRevenueRentVehicle = await _paymentRentVehicleRepository.getPaymentRentVehicleByDate(userId);
            var listLossCost = await _lossCostVehicleRepository.GetLossCostVehicleByDate(userId);
            var listExpenseRentDriver = await _historyRentDriverRepository.GetRentDetailsWithTotalForOwner();

            var revenue = new RevenueDTO
            {
                revenueTicketDTOs = new List<RevenueTicketDTO>{
                    new RevenueTicketDTO
                    {
                        listTicket = listRenvenueTicket.listTicket
                    }
                },
                totalLossCosts = new List<TotalLossCost> {
                    new TotalLossCost
                    {
                        listLossCostVehicle = listLossCost.listLossCostVehicle
                    }
                },
                totalPayementRentDrivers = new List<TotalPayementRentDriver> {
                    new TotalPayementRentDriver
                    {
                        PaymentRentDriverDTOs = listExpenseRentDriver.PaymentRentDriverDTOs,
                    }
                },
                totalPaymentRentVehicleDTOs = new List<TotalPaymentRentVehicleDTO> {
                    new TotalPaymentRentVehicleDTO
                    {
                       PaymentRentVehicelDTOs = listRevenueRentVehicle.PaymentRentVehicelDTOs,
                    }
                },
                totalRevenue = listRenvenueTicket.total + listRevenueRentVehicle.Total - listExpenseRentDriver.Total - listLossCost.TotalCost

            };
            return revenue;
        }


        // update revenue version 2
        public async Task<RevenueDTO> RevenueStatisticUpdate(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {

            if (!startDate.HasValue || !endDate.HasValue)
            {
                var now = DateTime.Now;
                startDate ??= new DateTime(now.Year, now.Month, 1);
                endDate ??= new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            }
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);


            if (userId <= 0)
            {
                throw new Exception("Invalid user ID from token.");
            }

            var listRenvenueTicket = await _ticketRepository.getRevenueTicketUpdate(startDate, endDate, vehicleId, userId);
            var listRevenueRentVehicle = await _paymentRentVehicleRepository.getPaymentRentVehicleByDateUpdate(startDate, endDate, vehicleId, userId);
            var listLossCost = await _lossCostVehicleRepository.GetLossCostVehicleByDateUpdate(startDate, endDate, vehicleId, userId);
            var listExpenseRentDriver = await _historyRentDriverRepository.GetRentDetailsWithTotalForOwnerUpdate(startDate, endDate, vehicleId);

            var revenue = new RevenueDTO
            {
                revenueTicketDTOs = new List<RevenueTicketDTO>{
                    new RevenueTicketDTO
                    {
                        listTicket = listRenvenueTicket.listTicket,
                        total = listRenvenueTicket.total
                    }
                },
                totalLossCosts = new List<TotalLossCost> {
                    new TotalLossCost
                    {
                        listLossCostVehicle = listLossCost.listLossCostVehicle,
                        TotalCost = listLossCost.TotalCost

                    }
                },
                totalPayementRentDrivers = new List<TotalPayementRentDriver> {
                    new TotalPayementRentDriver
                    {
                        PaymentRentDriverDTOs = listExpenseRentDriver.PaymentRentDriverDTOs,
                        Total = listExpenseRentDriver.Total
                    }
                },
                totalPaymentRentVehicleDTOs = new List<TotalPaymentRentVehicleDTO> {
                    new TotalPaymentRentVehicleDTO
                    {
                       PaymentRentVehicelDTOs = listRevenueRentVehicle.PaymentRentVehicelDTOs,
                       Total = listRevenueRentVehicle.Total
                    }
                },
                totalRevenue = listRenvenueTicket.total + listRevenueRentVehicle.Total - listExpenseRentDriver.Total - listLossCost.TotalCost

            };
            return revenue;
        }
    }
}

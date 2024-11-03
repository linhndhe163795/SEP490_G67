using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.LossCostDTOs.LossCostVehicelDTOs;
using MyAPI.Infrastructure.Interfaces;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LossCostVehicleController : ControllerBase
    {
        private readonly ILossCostVehicleRepository _lossCostCarRepository;
        private readonly IMapper _mapper;
        public LossCostVehicleController(ILossCostVehicleRepository lossCostCarRepository, IMapper mapper)
        {
            _lossCostCarRepository = lossCostCarRepository;
            _mapper = mapper;
        }
        [HttpGet("lossCostCar/vehicleId/startDate/endDate")]
        public async Task<IActionResult> lossCostVehicleByDate(int? vehicleId,  DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (vehicleId == null && startDate == null && endDate == null) 
                {
                    var listLossCost = await _lossCostCarRepository.GetAllLostCost();
                    return Ok(listLossCost);
                }
                else
                {
                    var listLostCostByDate = await _lossCostCarRepository.GetLossCostVehicleByDate(vehicleId, startDate, endDate);
                    return Ok(listLostCostByDate);
                }


            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.TicketDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using System.Reflection;


namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IMapper _mapper;
        private readonly IVehicleRepository _vehicleRepository;
        public TicketController(ITicketRepository ticketRepository, GetInforFromToken getInforFromToken, IMapper mapper, IVehicleRepository vehicleRepository)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _getInforFromToken = getInforFromToken;
            _vehicleRepository = vehicleRepository;
        }
        [Authorize]
        [HttpPost("bookTicket/{tripDetailsId}/{date}")]
        public async Task<IActionResult> createTicket([FromBody] BookTicketDTOs ticketDTOs, int tripDetailsId, string? promotionCode, int numberTicket, DateTime date)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);

                var ticketId = await _ticketRepository.CreateTicketByUser(promotionCode, tripDetailsId, ticketDTOs, userId, numberTicket, date);

                return Ok(new { ticketId, ticketDetails = ticketDTOs });
            }
            catch (Exception ex)
            {
                return BadRequest("createTicket: " + ex.Message);
            }
        }
        [Authorize(Roles = "Driver, Staff")]
        [HttpPost("createTicketFromDriver/vehicleId/numberTicket")]
        public async Task<IActionResult> creatTicketFromDriver([FromBody] TicketFromDriverDTOs ticketFromDriver, int numberTicket)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var driverId = _getInforFromToken.GetIdInHeader(token);
                var vehicleId = await _vehicleRepository.getVehicleByDriver(driverId);
                int pointStartId = Int32.Parse(ticketFromDriver.PointStart);
                int pointEndId = Int32.Parse(ticketFromDriver.PointEnd);
                var pointStartString = await _vehicleRepository.getPointStart(pointStartId);
                var pointEndString = await _vehicleRepository.getPointEnd(pointEndId);
                var ticketFromDrivers = new TicketFromDriverDTOs
                {
                    PointEnd = pointEndString,
                    PointStart = pointStartString
                };
                var priceTrip = await _ticketRepository.GetPriceFromPoint(ticketFromDrivers, vehicleId);
                await _ticketRepository.CreatTicketFromDriver(priceTrip, vehicleId, ticketFromDriver, driverId, numberTicket);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("getPriceFromPoint/pointStart/pointEnd/vehicleId")]
        public async Task<IActionResult> getPriceFromPoint(int pointStart, int pointEnd)
        {
            try
            {
                  string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var driverId = _getInforFromToken.GetIdInHeader(token);
                var vehicleId = await _vehicleRepository.getVehicleByDriver(driverId);
                var pointStartString = await _vehicleRepository.getPointStart(pointStart);
                var pointEndString = await _vehicleRepository.getPointEnd(pointEnd);

                var ticketFromDriver = new TicketFromDriverDTOs
                {
                    PointEnd = pointEndString,
                    PointStart = pointStartString
                };
                var priceTrip = await _ticketRepository.GetPriceFromPoint(ticketFromDriver, vehicleId);
                return Ok(priceTrip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("createTicketForRentCar")]
        public async Task<IActionResult> CreateTicketForRentCar([FromBody] AddTicketForRentCarDTO addTicketForRentCarDTO)
        {
            try
            {
                await _ticketRepository.AcceptOrDenyRequestRentCar(addTicketForRentCarDTO);

                return Ok("Ticket created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetTravelCarByRequest/{requestId}/startDate/endDate")]
        public async Task<IActionResult> GetVehiclesByRequestId(int requestId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var vehicles = await _ticketRepository.GetVehiclesByRequestIdAsync(requestId, startDate, endDate);

                if (vehicles == null || !vehicles.Any())
                {
                    return Ok(new List<object>());
                }

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to retrieve vehicles.", Details = ex.Message });
            }
        }
        [HttpPost("AssignTravelCarForRent")]
        public async Task<IActionResult> UpdateVehicleInRequest(int vehicleId, int requestId)
        {
            try
            {

                var result = await _ticketRepository.UpdateVehicleInRequestAsync(vehicleId, requestId);

                if (result)
                {
                    return Ok(new { Message = "Vehicle updated successfully for the request." });
                }
                else
                {
                    return NotFound(new { Message = "Request not found or update failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update vehicle for the request.", Details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> getListTicket()
        {
            try
            {
                var listTicket = await _ticketRepository.getAllTicket();
                if (listTicket == null) return NotFound();
                var listTickerMapper = _mapper.Map<List<ListTicketDTOs>>(listTicket);
                return Ok(listTickerMapper);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTicket: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff, Driver")]
        [HttpGet("tickeNotPaid/{vehicleId}")]
        public async Task<IActionResult> getListTicketNotPaid(int vehicleId)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var role = _getInforFromToken.GetRoleFromToken(token);
                if (role == "Driver" && await _vehicleRepository.checkDriver(vehicleId, userId) == false)
                {
                    return NotFound("Not Authentication");
                }
                var listTicket = await _ticketRepository.GetListTicketNotPaid(vehicleId);
                if (listTicket == null) return NotFound();
                return Ok(listTicket);
            }
            catch (Exception ex)
            {
                return BadRequest("getListTicket: " + ex.Message);
            }
        }
        [Authorize]
        [HttpGet("ticketById/{ticketId}")]
        public async Task<IActionResult> getTicketByTicketId(int ticketId)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var ticketById = await _ticketRepository.getTicketDetailsById(ticketId, userId);
                return Ok(ticketById);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("updateTicket/{id}")]
        public async Task<IActionResult> updateTicketByTicketId(int id, TicketUpdateDTOs ticket)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                await _ticketRepository.updateTicketByTicketId(id, userId, ticket);
                return Ok("Update Ticket Success !");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Driver")]
        [HttpPost("updateStatusticketNotPaid/id")]
        public async Task<IActionResult> updateStatusTicketNotPaid(int id)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var driverId = _getInforFromToken.GetIdInHeader(token);
                await _ticketRepository.UpdateStatusTicketNotPaid(id, driverId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("deleteTicket/{id}")]
        public async Task<IActionResult> deleteTicketById(int id)
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                await _ticketRepository.deleteTicketByTicketId(id, userId);
                return Ok("delete success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "VehicleOwner, Staff")]
        [HttpGet("RevenueTicket")]
        public async Task<IActionResult> getRevenueTicket()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var respone = await _ticketRepository.getRevenueTicket(userId);
                return Ok(respone);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //update version 2
        [HttpGet("RevenueTicket/startDate/endDate/vehicleId")]
        public async Task<IActionResult> getRevenueTicketV2(DateTime? startDate, DateTime? endDate, int? vehicleId)
        {
            try
            {
                
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var respone = await _ticketRepository.getRevenueTicketUpdate(startDate, endDate, vehicleId, userId);
                return Ok(respone);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //end update
        [HttpPost("deleteTicketTimeOut/{ticketId}")]
        public async Task<IActionResult> deleteTicketByTicketId(int ticketId)
        {
            try
            {
                var requests = await _ticketRepository.deleteTicketTimeOut(ticketId);
                if (requests)
                {
                    return Ok(requests);
                }
                else
                {
                    return NotFound("Id not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "deleteTicketByTicketId failed", Details = ex.Message });
            }
        }
        [HttpGet("listTicketByUserId")]
        public async Task<IActionResult> getListTicketByUserId()
        {
            try
            {
                string token = Request.Headers["Authorization"];
                if (token.StartsWith("Bearer"))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required.");
                }
                var userId = _getInforFromToken.GetIdInHeader(token);
                var list = await _ticketRepository.GetTicketByUserId(userId);
                return Ok(list);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

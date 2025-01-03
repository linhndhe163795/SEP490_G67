﻿using AutoMapper;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.PromotionDTOs;
using MyAPI.Helper;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionUserRepository _promotionUserRepository;
        private readonly GetInforFromToken _getInforFromToken;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public PromotionController(IPromotionRepository promotionRepository, GetInforFromToken getInforFromToken, IMapper mapper, IPromotionUserRepository promotionUserRepository, IUserRepository userRepository)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
            _getInforFromToken = getInforFromToken;
            _promotionUserRepository = promotionUserRepository;
            _userRepository = userRepository;
        }
        [Authorize(Roles = "Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAllPromotion()
        {
            try
            {
                var listPromotion = await _promotionRepository.GetAll();

                var sortedPromotion = listPromotion.OrderByDescending(x => x.Id).ToList();

                var listPromotionMapper = _mapper.Map<List<PromotionDTO>>(sortedPromotion);
                return Ok(listPromotionMapper);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getPromotionById/{promotionId}")]
        public async Task<IActionResult> getPromotionByPromotionId(int promotionId)
        {
            try
            {
                var promotion = await _promotionRepository.getPromotionByPromotionId(promotionId);
                return Ok(promotion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("getPromotionById")]
        public async Task<IActionResult> GetPromotionByUser()
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
                var listPromtion = await _promotionRepository.getPromotionUserById(userId);
                if (listPromtion == null) return NotFound();
                return Ok(listPromtion);
            }
            catch (Exception ex)
            {
                return BadRequest("GetPromotionByUser: " + ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("CreatePromotion")]
        public async Task<IActionResult> CreatePromotion([FromForm] PromotionPost promotionDTO)
        {
            try
            {
                if (promotionDTO == null)
                    return BadRequest("Promotion data is required.");
              
                var createdPromotion = await _promotionRepository.CreatePromotion(promotionDTO);
                return CreatedAtAction(nameof(GetPromotionByUser), new { id = createdPromotion.CodePromotion }, createdPromotion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Staff")]
      
        [HttpPost("updatePromotion/id")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromForm] PromotionDTO promotionDTO)
        
        {
            try
            {
                var getPromotionById = await _promotionRepository.Get(id);
                if (getPromotionById == null) return NotFound("Not found promotion had id = " + id);
                getPromotionById.Description = promotionDTO.Description;
                getPromotionById.Discount = promotionDTO.Discount;
                getPromotionById.ExchangePoint = promotionDTO.ExchangePoint;
                getPromotionById.CodePromotion = promotionDTO.CodePromotion;
                getPromotionById.ImagePromotion = promotionDTO.ImagePromotion;
                getPromotionById.UpdateAt = DateTime.Now;
                getPromotionById.UpdateBy = 1;
                getPromotionById.StartDate = promotionDTO.StartDate;
                getPromotionById.EndDate = promotionDTO.EndDate;
                await _promotionRepository.Update(getPromotionById);
                return Ok(promotionDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("deletePromotion/id")]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            try
            {
                var getPromotionById = await _promotionRepository.Get(id);
                await _promotionUserRepository.DeletePromotionUser(id);
                if (getPromotionById == null) return NotFound("Not found promotion had id = " + id);
                await _promotionRepository.Delete(getPromotionById);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("givePromotionToUser/PromotionId/userId")]
        public async Task<IActionResult> GivePromotionToUser(int PromotionId, int userId)
        {
            try
            {
                if (PromotionId <= 0)
                {
                    return BadRequest("PromotionId must be greater than zero.");
                }

                if (userId <= 0)
                {
                    return BadRequest("UserId must be greater than zero.");
                }
                var getPromotionById = await _promotionRepository.Get(PromotionId);
                if (getPromotionById == null) return NotFound("Not found promotion had id = " + PromotionId);
                var user = await _userRepository.Get(userId);

                if (user == null) return NotFound("Not found user had id = " + userId);
                PromotionUser pu = new PromotionUser
                {
                    DateReceived = DateTime.UtcNow,
                    UserId = userId,
                    PromotionId = PromotionId
                };
                await _promotionUserRepository.Add(pu);
                return Ok("Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("givePromotionAllUser")]
        public async Task<IActionResult> GivePromotionAllUser([FromForm] PromotionPost promotionDTO)
        {
            try
            {
                var promotionMapper = _mapper.Map<Promotion>(promotionDTO);
                await _promotionRepository.Add(promotionMapper);
                await _promotionUserRepository.AddPromotionAllUser(promotionMapper.Id);

                return Ok(promotionDTO);
            }
            catch (Exception ex)
            {
                return BadRequest("GivePromotionAllUser: " + ex.Message);
            }
        }
        [HttpPost("deletePromotionAfterPayment/{userId}/{promotionId}")]
        public async Task<IActionResult> deletePromotionAfterPayment(int userId, int promotionId)
        {
            try
            {
                var requests = await _promotionUserRepository.DeletePromotionAfterPayment(userId, promotionId);
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
                return BadRequest(new { Message = "deletePromotionAfterPayment failed", Details = ex.Message });
            }
        }
        [Authorize]
        [HttpPost("exchangePromtion/{promotionId}")]
        public async Task<IActionResult> getPromotionExchange(int promotionId)
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
                await _promotionRepository.exchangePromotion(userId, promotionId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("listPromtionCanChange")]
        public async Task<IActionResult> getListPromotionUserCanChange()
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
                var listPromotionUserCanChange = await _promotionRepository.listPromotionCanChange(userId);
                return Ok(listPromotionUserCanChange);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

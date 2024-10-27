﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyAPI.DTOs.PromotionDTOs;
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
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public PromotionController(IPromotionRepository promotionRepository, IMapper mapper, IPromotionUserRepository promotionUserRepository, IUserRepository userRepository)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
            _promotionUserRepository = promotionUserRepository;
            _userRepository = userRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPromotion() 
        {
            try
            {
                var listPromotion = await _promotionRepository.GetAll();
                var listPromotionMapper = _mapper.Map<List<PromotionDTO>>(listPromotion);
                return Ok(listPromotionMapper);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("updatePromotion/id")]
        public async Task<IActionResult> UpdatePromotion(int id, PromotionDTO promotionDTO)
        {
            try
            {
                var getPromotionById = await _promotionRepository.Get(id);
                if (getPromotionById == null) return NotFound("Not found promotion had id = " + id);
                getPromotionById.Description = promotionDTO.Description;
                getPromotionById.Discount = promotionDTO.Discount;
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
        [HttpDelete("deletePromotion/id")]
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
        [HttpPost("givePromotionToUser/PromotionId/userId")]
        public async Task<IActionResult> GivePromotionToUser(int PromotionId, int userId)
        {
            try
            {
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
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("givePromotionAllUser")]
        public async Task<IActionResult> GivePromotionAllUser(PromotionDTO promotionDTO)
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
    }
}
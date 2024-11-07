using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public PromotionController(IPromotionRepository promotionRepository, GetInforFromToken getInforFromToken,IMapper mapper, IPromotionUserRepository promotionUserRepository, IUserRepository userRepository)
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
                var listPromotionMapper = _mapper.Map<List<PromotionDTO>>(listPromotion);
                return Ok(listPromotionMapper);
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
        [HttpPut("updatePromotion/id")]
        public async Task<IActionResult> UpdatePromotion(int id, [FromForm] PromotionDTO promotionDTO, IFormFile? imageFile)
        {
            try
            {
                var getPromotionById = await _promotionRepository.Get(id);
                if (getPromotionById == null) return NotFound("Not found promotion had id = " + id);
                getPromotionById.Description = promotionDTO.Description;
                getPromotionById.Discount = promotionDTO.Discount;
                getPromotionById.CodePromotion = promotionDTO.CodePromotion;
                getPromotionById.ImagePromotion = promotionDTO.ImagePromotion;
                getPromotionById.UpdateAt = DateTime.Now;
                getPromotionById.UpdateBy = 1;
                getPromotionById.StartDate = promotionDTO.StartDate;
                getPromotionById.EndDate = promotionDTO.EndDate;
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                    var fileExtension = Path.GetExtension(imageFile.FileName);
                    var newFileName = $"{fileName}_{DateTime.Now.Ticks}{fileExtension}";
                    var filePath = Path.Combine("wwwroot/uploads", newFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    getPromotionById.ImagePromotion = $"/uploads/{newFileName}";
                }
                await _promotionRepository.Update(getPromotionById);
                return Ok(promotionDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Staff")]
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
        [Authorize(Roles = "Staff")]
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
        [Authorize(Roles = "Staff")]
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

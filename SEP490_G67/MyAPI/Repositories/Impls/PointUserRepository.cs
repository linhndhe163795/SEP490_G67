﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyAPI.DTOs.PointUserDTOs;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;

namespace MyAPI.Repositories.Impls
{
    public class PointUserRepository : GenericRepository<PointUser>, IPointUserRepository
    {
        private readonly IMapper _mapper;
        public PointUserRepository(SEP490_G67Context _context, IMapper mapper) : base(_context)
        {
            _mapper = mapper;
        }

        public async Task addNewPointUser( int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new Exception("Invalid User ID.");
                }
                var addNewPointUser = new PointUser
                {
                    UserId = userId,
                    Points = 0,
                    PointsMinus = 0,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    Date = DateTime.Now,
                };
                _context.PointUsers.Add(addNewPointUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> addPointUser(PointUserAddDTO pointUserAddDTO)
        {

            try
            {
                if (pointUserAddDTO == null)
                {
                    throw new ArgumentNullException(nameof(pointUserAddDTO), "PointUserAddDTO is required.");
                }

                if (pointUserAddDTO.UserId < 0)
                {
                    throw new Exception("Invalid User ID.");
                }

                if (pointUserAddDTO.PaymentId < 0)
                {
                    throw new Exception("Invalid Payment ID.");
                }
                var addPoint = new PointUser
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = pointUserAddDTO.CreatedBy,
                    PaymentId = pointUserAddDTO.PaymentId,
                    UpdateBy = pointUserAddDTO.UpdateBy,
                    Points = pointUserAddDTO.Points,
                    PointsMinus = pointUserAddDTO.PointsMinus,
                    Date = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    UserId = pointUserAddDTO.UserId
                };
                await _context.PointUsers.AddAsync(addPoint);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PointUserDTOs> getPointUserById(int userId)
        {
            try
            {
                var pointOfUser = await _context.PointUsers.Where(x => x.UserId == userId).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                PointUserDTOs puds = new PointUserDTOs
                {
                    id = 1,
                    Points = pointOfUser?.Points ?? 0
                };
                return puds;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<PointHistoryDTO>> getPointHistoryByUserId(int userId)
        {
            try
            {
                var pointHistory = await _context.PointUsers
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new PointHistoryDTO
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Points = x.Points,
                        MinusPoints = x.PointsMinus,
                        Date = x.CreatedAt
                    })
                    .ToListAsync();

                if (pointHistory == null || !pointHistory.Any())
                {
                    throw new Exception("No history founded!");
                }

                return pointHistory;
            }
            catch (Exception ex)
            {
                throw new Exception("getPointHistoryByUserId: " + ex.Message);
            }
        }


        public async Task<bool> updatePointUser(int userId, PointUserUpdateDTO pointUserUpdateDTO)
        {
            var checkPoint = await _context.PointUsers.FirstOrDefaultAsync(s => s.UserId == userId);
            if (checkPoint != null)
            {
                checkPoint.Points = checkPoint.Points + pointUserUpdateDTO.Points;
                checkPoint.Date = DateTime.Now;
                checkPoint.UpdateAt = DateTime.Now;
                checkPoint.UpdateBy = pointUserUpdateDTO.UpdateBy;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                throw new Exception("Update point user!!!");
            }
        }
    }
}

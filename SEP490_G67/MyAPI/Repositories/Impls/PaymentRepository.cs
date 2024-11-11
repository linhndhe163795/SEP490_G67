using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyAPI.Infrastructure.Interfaces;
using MyAPI.Models;
using static System.Net.WebRequestMethods;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using MyAPI.DTOs.PaymentDTOs;
using MyAPI.Helper;
using MyAPI.DTOs;
using MyAPI.DTOs.PointUserDTOs;
using Microsoft.EntityFrameworkCore;

namespace MyAPI.Repositories.Impls
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GetInforFromToken _tokenHelper;
        private readonly SendMail _sendMail;
        private readonly IPointUserRepository _pointUserRepository;
        private readonly ITicketRepository _ticketRepository;
        public PaymentRepository(SEP490_G67Context _context, GetInforFromToken tokenHelper
            , IHttpContextAccessor httpContextAccessor
            , SendMail sendMail, IPointUserRepository pointUserRepository, ITicketRepository ticketRepository) : base(_context)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
            _sendMail = sendMail;
            _pointUserRepository = pointUserRepository;
            _ticketRepository = ticketRepository;   
        }

        private const string API_KEY = "AK_CS.ab1265c09b8511ef8a02890bf6befcfe.ootjjrZYN3szbLfojxUvMfPWNFSzbxFeJd7ScLfGFTnvGNu4goxy9iKc1YZU9d2n01TKhP1D";
        private const string API_GET_PAID = "https://oauth.casso.vn/v2/transactions";


        public async Task<bool> checkHistoryPayment(int amout, string description, string codePayment, int ticketID, int typePayment, string email)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            int userId = _tokenHelper.GetIdInHeader(token);

            if (userId == -1)
            {
                throw new Exception("Invalid user ID from token.");
            }
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("apikey", API_KEY);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await client.GetAsync(API_GET_PAID);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"HTTP error! Status: {response.StatusCode}");
                        return false;
                    }

                    var data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Full Response Data: " + data);

                    var json = JObject.Parse(data);
                    var records = json["data"]?["records"];

                    if (records != null)
                    {
                        foreach (var record in records)
                        {
                            var recordDescription = record["description"]?.ToString().Trim().ToLower();
                            Console.WriteLine("Record Description: " + recordDescription);
                            var recordAmount = record["amount"]?.ToObject<int>();
                            Console.WriteLine("Record Amount: " + recordAmount);

                            var datePayment = record["when"]?.ToString();
                            Console.WriteLine("Record date: " + datePayment);

                            if (!string.IsNullOrEmpty(recordDescription) &&
                            recordDescription.Contains(description.Trim().ToLower()) &&
                            recordAmount == amout)
                            {
                                Console.WriteLine("Thanh toán thành công!");
                                var paymentDTO = new PaymentAddDTO
                                {
                                    UserId = userId,
                                    Code = codePayment,
                                    Description = recordDescription,
                                    Price = (decimal)recordAmount,
                                    TicketId = ticketID,
                                    TypeOfPayment = typePayment,
                                    Time = DateTime.Parse(datePayment)

                                };
                                var paymentResult = await addPayment(paymentDTO);
                                if (paymentResult == null)
                                {
                                    throw new Exception("Add Payment Fails!!");
                                }

                                SendMailDTO mail = new SendMailDTO
                                {
                                    FromEmail = "nhaxenhanam@gmail.com",
                                    Password = "vzgq unyk xtpt xyjp",
                                    ToEmail = email,
                                    Subject = "Thông báo về việc thanh toán vé xe ",
                                    Body = "Thanh toán đơn hàng thành công!!"
                                };

                                var checkMail = await _sendMail.SendEmail(mail);
                                if (!checkMail)
                                {
                                    throw new Exception("Send mail fail!!");
                                }


                                int amountPoint = 0;
                                if (paymentResult.Price.HasValue)
                                {
                                    amountPoint = (int)Math.Round(paymentResult.Price.Value * 0.1m);  
                                }
                                else
                                {
                                    throw new Exception("Price is null");
                                }

                                var checkPointUserExits = await _context.PointUsers.FirstOrDefaultAsync(s => s.UserId == userId);

                                if (checkPointUserExits == null)
                                {
                                    var pointUser = new PointUserAddDTO
                                    {
                                        CreatedBy = userId,
                                        UserId = userId,
                                        PaymentId = paymentResult.PaymentId,
                                        Points = amountPoint,
                                        PointsMinus = 0,
                                        UpdateBy = userId,
                                    };
                                   await _pointUserRepository.addPointUser(pointUser);
                                }else
                                {
                                    var updatePointDTO = new PointUserUpdateDTO
                                    {
                                        Points = amountPoint,
                                        UpdateBy = userId,
                                    };
                                    await _pointUserRepository.updatePointUser(userId, updatePointDTO);
                                }

                                await _ticketRepository.UpdateStatusTicketForPayment(ticketID);
                                return true;
                            }
                        }
                    }

                    Console.WriteLine("Thanh toán không thành công.");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during payment check: " + ex.Message);
                    return false;
                }
            }
        }

        public async Task<Payment> addPayment(PaymentAddDTO paymentAddDTO)
        {
            try
            {
                var payment = new Payment
                {
                    Code = paymentAddDTO.Code,
                    CreatedAt = DateTime.Now,
                    CreatedBy = paymentAddDTO.UserId,
                    Description = paymentAddDTO.Description,
                    Price = paymentAddDTO.Price,
                    TicketId = paymentAddDTO.TicketId,
                    TypeOfPayment = paymentAddDTO.TypeOfPayment,
                    UpdateAt = DateTime.Now,
                    UpdateBy = paymentAddDTO.UserId,
                    Time = paymentAddDTO.Time,
                };

                await _context.AddAsync(payment);
                await _context.SaveChangesAsync();
                return payment;
            }
            catch (Exception ex)
            {
                throw new Exception("UpdateVehicleByStaff: " + ex.Message);
            }
            
        }

        public async Task<int> GenerateRandomNumbers()
        {
            Random random = new Random();
            return random.Next(100000, 1000000);
        }
    }
}


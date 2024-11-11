using MyAPI.DTOs.PaymentDTOs;
using MyAPI.Models;

namespace MyAPI.Infrastructure.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<bool> checkHistoryPayment(int amout, string description, string codePayment, int ticketID, int typePayment, string email);

        Task<Payment> addPayment(PaymentAddDTO paymentAddDTO);

        Task<int> GenerateRandomNumbers();
    }
}

using Infra;
using Infra.Repositories.Interfaces;
using Service.DTOs.TransactionDTOs;

namespace Service.HolidayServices;

public class TransactionService(ITransactionRepo repo)
{
    public async Task<List<HolidayTransactionDto>> GetHistory(Guid accountId)
    {
        if (accountId == Guid.Empty)
            throw new ValidationException("Account id is required");

        var transactions = await repo.GetByAccount(accountId);

        return transactions.Select(t => new HolidayTransactionDto
        {
            Id = t.Id,
            HolidayAccountId = t.HolidayAccountId,
            HolidayEarningId = t.HolidayEarningId,
            AmountHours = t.AmountHours,
            TransactionDate = t.TransactionDate,
            TransactionType = t.TransactionType,
            Description = t.Description
        }).ToList();
    }
    
}
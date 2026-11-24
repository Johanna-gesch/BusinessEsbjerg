using Infra;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;
using Service.DTOs.TransactionDTOs;

namespace Service.HolidayServices;

/**
 * This service class retrieves the history of earned, used, transferred, and adjusted hours.
 */
public class TransactionService(
    ITransactionRepo transactionRepo, 
    IAccountRepo accountRepo)
{
    // Retrieves the account's transactions and converts each database entity
    // into a DTO before returning the history to the caller.
    public async Task<List<HolidayTransactionDto>> GetHistory(Guid accountId)
    {
        if (accountId == Guid.Empty)
            throw new ValidationException("Account id is required");

        var transactions = await transactionRepo.GetByAccount(accountId);
        return transactions.Select(ToDto).ToList();
    }

    // Validates the request, creates and saves a transaction entity,
    // then converts it to a DTO for the response.
    public async Task<HolidayTransactionDto> Create(CreateHolidayTransactionDto dto)
    {
        if (dto.HolidayAccountId == Guid.Empty)
            throw new ValidationException("Account id is required");
        if (dto.AmountHours == 0)
            throw new ValidationException("Amount hours cannot be 0");
        if (dto.TransactionDate == default)
            throw new ValidationException("Transaction date is required");

        switch (dto.TransactionType)
        {
            case HolidayTransactionType.Earned:
            case HolidayTransactionType.TransferredIn:
                if (dto.AmountHours < 0)
                    throw new ValidationException("Earned and TransferredIn must be positive");
                break;
            
            case HolidayTransactionType.Used:
            case HolidayTransactionType.TransferredOut:
            case HolidayTransactionType.PaidOut:
                if (dto.AmountHours > 0)
                    throw new ValidationException("Used, TransferredOut and PaidOut must be negative");
                break;

            case HolidayTransactionType.Adjustment:
                if (string.IsNullOrWhiteSpace(dto.Description))
                    throw new ValidationException("Adjustment requires a description");
                break;
        }

        if (await accountRepo.GetByIdAsync(dto.HolidayAccountId) is null)
            throw new NotFoundException("Holiday account not found");
        
        // Map the validated request DTO to the entity that is stored in the database.
        var entity = new HolidayTransaction
        {
            Id = Guid.NewGuid(),
            HolidayAccountId = dto.HolidayAccountId,
            HolidayEarningId = dto.HolidayEarningId,
            AmountHours = dto.AmountHours,
            TransactionDate = dto.TransactionDate,
            TransactionType = dto.TransactionType,
            Description = dto.Description
        };
        
        await transactionRepo.Insert(entity);
        
        // Return a DTO instead of exposing the database entity to the caller.
        return ToDto (entity);
    }

    // Converts a database entity into the response DTO.
    // This keeps the persistence model separate from the data returned to the caller.
    private static HolidayTransactionDto ToDto (HolidayTransaction t) => new()
    {
        Id = t.Id,
        HolidayAccountId = t.HolidayAccountId,
        HolidayEarningId = t.HolidayEarningId,
        AmountHours = t.AmountHours,
        TransactionDate = t.TransactionDate,
        TransactionType = t.TransactionType,
        Description = t.Description
    };
}
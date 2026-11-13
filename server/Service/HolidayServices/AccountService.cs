using System.ComponentModel.DataAnnotations;
using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Repositories.Interfaces;
using Service.DTOs.HolidayDTOs;
using ValidationException = Infra.ValidationException;

namespace Service.HolidayServices;

public class AccountService(IAccountRepo accountRepo)
{
    public async Task<HolidayAccount> CreateAccountAsync(CreateHolidayAccountDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ValidationException("User Id cannot be null.");

        if (!Enum.IsDefined(typeof(HolidayType), dto.Type))
            throw new ValidationException("Invalid account type.");
        
        var account = new HolidayAccount
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = dto.Type
        };

        await accountRepo.InsertAsync(account);

        return account;
    }

    public async Task<HolidayAccount> GetAccountById(Guid id)
    {
        
        if (id == Guid.Empty)
            throw new ValidationException("Account Id cannot be empty.");

        return await accountRepo.GetByIdAsync(id) ??
               throw new NotFoundException($"Account with this Id {id} is not found.");
    }

    public async Task<ICollection<HolidayAccount>> GetAccountsForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException("User Id cannot be empty.");

        return await accountRepo.GetUserAccountsAsync(userId);
    }
}
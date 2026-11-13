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
    public async Task CreateDefaultAccountsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ValidationException("User Id cannot be empty.");

        var accounts = new[]
        {
            new HolidayAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = HolidayType.Week1_5
            },
            new HolidayAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = HolidayType.Week6
            }
        };

        foreach (var account in accounts)
        {
            await accountRepo.InsertAsync(account);
        }
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
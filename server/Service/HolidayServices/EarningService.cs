using System.ComponentModel.DataAnnotations;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Repositories.Interfaces;

namespace Service.HolidayServices;

public class EarningService(
    IEarningRepo earningRepo,
    IAccountRepo accountRepo,
    IUserRepo userRepo)
{
    public async Task SaveEarnings()
    {
        throw new NotImplementedException();
    }

    public async Task CalculateEarnings(string userId)
    {
        var user = await userRepo.GetById(userId) ??
                   throw new ValidationException("User not found");

        var accounts = await accountRepo.GetUserAccountsAsync(userId) ??
                       throw new ValidationException($"No accounts found for user with id: {userId}");

        var week1_5Acc = accounts.SingleOrDefault(a => a.Type == HolidayType.Week1_5);

        var week6Acc = accounts.SingleOrDefault(a => a.Type == HolidayType.Week6);


    }
    
}
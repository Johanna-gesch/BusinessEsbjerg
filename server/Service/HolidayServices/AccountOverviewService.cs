using Infra.Entities;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;
using Service.DTOs;

namespace Service.HolidayServices;

public class AccountOverviewService (IUserRepo userRepo, IAccountOverviewRepo overviewRepo)
{
    public async Task<List<EmployeebalanceDto>> GetOverview(DateOnly date)
    {
        var users = await userRepo.GetAll();
        var accounts = await overviewRepo.GetAccounts();
        var earnings = await overviewRepo.GetEarningsValidOn(date);
        var transactions = await overviewRepo.GetEarningTransactionsUpTo(date);
        
        var transactionHoursByEarning = transactions
            .Where(t => t.HolidayEarningId != null
                        && t.TransactionType != HolidayTransactionType.Earned)
            .GroupBy(t => t.HolidayEarningId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.AmountHours));

        var accountsById = accounts.ToDictionary(a => a.Id);

        // The remaining hours per account.
        var hoursByAccount = new Dictionary<Guid, decimal>();

        foreach (var earning in earnings)
        {
            if (!accountsById.TryGetValue(earning.HolidayAccountId, out var account))
                continue;

            // Week 6 earnings become available on 1 May the year after they were earned.
            if (account.Type == HolidayType.Week6
                && date < new DateOnly(earning.EarnedDate.Year + 1, 5, 1))
                continue;

            var remaining = earning.EarnedHours
                            + transactionHoursByEarning.GetValueOrDefault(earning.Id);

            hoursByAccount[account.Id] = hoursByAccount.GetValueOrDefault(account.Id) + remaining;
        }

        var accountsByUser = accounts.ToLookup(a => a.UserId);

        var result = new List<EmployeebalanceDto>();

        foreach (var user in users.Where(u => u.IsActive).OrderBy(u => u.Username))
        {
            var row = new EmployeebalanceDto
            {
                UserId = user.UserId,
                Username = user.Username
            };

            foreach (var account in accountsByUser[user.UserId])
            {
                var hours = decimal.Round(hoursByAccount.GetValueOrDefault(account.Id), 2);

                if (account.Type == HolidayType.Week1_5)
                    row.Week1_5Hours = hours;
                else if (account.Type == HolidayType.Week6)
                    row.Week6Hours = hours;
            }

            result.Add(row);
        }

        return result;
    }
    
}
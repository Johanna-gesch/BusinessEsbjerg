using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Repositories.Interfaces;
using Service.DTOs.HolidayDTOs;
using Service.HolidayServices.Earnings;

namespace Service.HolidayServices;

/**
 * This class is responsible for calculating an employee's available holiday hours.
 */
public class HolidayBalanceService(
    IEarningRepo earningRepo,
    ITransactionRepo transactionRepo,
    IAccountRepo accountRepo,
    WorkScheduleService workScheduleService,
    EarningService earningService)
{
    /*
     * This retrieves the account, its earnings and its transactions.
     * It then calculates the remaining hours for each eligible earning.
     * It uses the account's holiday earnings,
     * transactions linked to those earnings,
     * earning dates and expiry dates,
     * and the availability rule for week 6.
     */
    public async Task<decimal> GetAvailableBalance(
        Guid accountId,
        DateOnly date)
    {
        if (accountId == Guid.Empty)
            throw new ValidationException("Account id is required.");

        var account = await accountRepo.GetByIdAsync(accountId)
                      ?? throw new NotFoundException("Holiday account not found.");

        return await CalculateBalance(account, date, includeUsedTransactions: true);
    }

    private async Task<decimal> CalculateBalance(
        HolidayAccount account,
        DateOnly date,
        bool includeUsedTransactions)
    {
        var earnings = await earningRepo.GetEarningsByUserAndAccount(account.UserId, account.Id);

        var transactions = await transactionRepo.GetByAccount(account.Id);

        decimal balance = 0;

        foreach (var earning in earnings)
        {
            // The earning must have been earned by the requested date
            // and must not have expired.
            if (earning.EarnedDate > date || earning.ExpiryDate < date)
                continue;

            // Week 6 earnings become available on May 1 in the year
            // after they were earned.
            if (account.Type == HolidayType.Week6 && date < new DateOnly(earning.EarnedDate.Year + 1, 5, 1))
            {
                continue;
            }

            balance += CalculateRemainingHours(earning, transactions, date, includeUsedTransactions);
        }

        return decimal.Round(balance, 2);
    }

    /*
     * This retrieves both accounts belong to an employee and calculates
     * their balances separately.
     * It returns the result as a DTO.
     */
    public async Task<UserHolidayBalanceDto> GetUserBalance(
        string userId,
        DateOnly date)
    {
        var accounts = await accountRepo.GetUserAccountsAsync(userId);

        var result = new UserHolidayBalanceDto();

        foreach (var account in accounts)
        {
            var balance = await GetAvailableBalance(account.Id, date);

            switch (account.Type)
            {
                case HolidayType.Week1_5:
                    result.Week1_5Hours = balance;
                    break;

                case HolidayType.Week6:
                    result.Week6Hours = balance;
                    break;
            }
        }

        return result;
    }

    /*
     * This method calculates the remaining hours by adding the transactions
     * to the earnings. If you have a transaction that used 10 hours, that will be
     * subtracted from the earnings.
     */
    private decimal CalculateRemainingHours(
        HolidayEarning earning,
        List<HolidayTransaction> transactions,
        DateOnly date,
        bool includeUsedTransactions)
    {
        var relatedTransactions = transactions.Where(t =>
            t.HolidayEarningId == earning.Id &&
            t.TransactionDate <= date &&
            t.TransactionType != HolidayTransactionType.Earned && 
            (includeUsedTransactions ||
             t.TransactionType != HolidayTransactionType.Used)
            );

        var transactionHours = relatedTransactions.Sum(t => t.AmountHours);

        return earning.EarnedHours + transactionHours;
    }

    /*
     * This returns true if a user has sufficient balance.
     */
    public async Task<bool> HasSufficientBalance(
        Guid accountId,
        decimal requiredHours,
        DateOnly date)
    {
        if (requiredHours < 0)
            throw new ValidationException(
                "Required hours can't be negative.");

        var availableBalance = await GetAvailableBalance(accountId, date);

        return availableBalance >= requiredHours;
    }

    /*
     * This method determines which individual earnings should cover an approved holiday request.
     * The hours that expire first are used first.
     */
    public async Task<List<HolidayTransaction>> AllocateHours(
    string userId,
    decimal requiredHours,
    DateOnly date)
{
    if (string.IsNullOrWhiteSpace(userId))
        throw new ValidationException("User id is required.");

    if (requiredHours <= 0)
        throw new ValidationException(
            "Required hours must be greater than zero.");

    // Retrieve both holiday accounts belonging to the employee.
    var accounts = await accountRepo.GetUserAccountsAsync(userId);

    // This tuple stores the earning being used, the account it belongs to, and the remaining hours in the earning.
    var availableEarnings = new List<(HolidayEarning Earning, Guid AccountId, decimal RemainingHours)>();

    foreach (var account in accounts)
    {
        var earnings = await earningRepo.GetEarningsByUserAndAccount(
            userId, account.Id);

        var transactions = await transactionRepo.GetByAccount(account.Id);

        var accountEarnings = earnings
            .Where(e =>
                e.EarnedDate <= date &&
                e.ExpiryDate >= date &&
                (account.Type != HolidayType.Week6 ||
                 date >= new DateOnly(e.EarnedDate.Year + 1, 5, 1)))
            .Select(e => new
            {
                Earning = e,
                RemainingHours = CalculateRemainingHours(
                    e, transactions, date, includeUsedTransactions: true)
            })
            .Where(x => x.RemainingHours > 0);

        availableEarnings.AddRange(
            accountEarnings.Select(x =>
                (x.Earning, account.Id, x.RemainingHours)));
    }

    // FIFO: consume earnings that expire first.
    var orderedEarnings = availableEarnings
        .OrderBy(x => x.Earning.ExpiryDate)
        .ThenBy(x => x.Earning.EarnedDate)
        .ToList();

    var availableBalance = orderedEarnings.Sum(x => x.RemainingHours);

    if (availableBalance < requiredHours)
        throw new ValidationException(
            $"Insufficient holiday hours. Available: {availableBalance:F2}, " +
            $"required: {requiredHours:F2}.");

    var allocations = new List<HolidayTransaction>();
    var hoursToAllocate = requiredHours;

    foreach (var item in orderedEarnings)
    {
        if (hoursToAllocate <= 0)
            break;

        var hoursFromEarning = Math.Min(
            item.RemainingHours,
            hoursToAllocate);

        allocations.Add(new HolidayTransaction
        {
            Id = Guid.NewGuid(),
            HolidayAccountId = item.AccountId,
            HolidayEarningId = item.Earning.Id,
            AmountHours = -hoursFromEarning,
            TransactionDate = date,
            TransactionType = HolidayTransactionType.Used,
            Description = "Holiday request"
        });

        hoursToAllocate -= hoursFromEarning;
    }

    return allocations;
}
    
    /*
     * This uses WorkScheduleService to see if a debt is below or equal to one week's worth of work.
     */
    public async Task<bool> IsDebtAllowed(
        string userId,
        decimal debtHours,
        DateOnly date)
    {
        if (debtHours < 0)
            throw new ValidationException(
                "Debt hours cannot be negative.");

        var weeklyHours = await workScheduleService.GetWeeklyHours(
            userId, date);

        return debtHours <= weeklyHours;
    }
    
    public async Task<decimal> GetProjectedBalance(
        Guid accountId,
        DateOnly checkpoint)
    {
        if (accountId == Guid.Empty)
            throw new ValidationException("Account id is required.");

        var account = await accountRepo.GetByIdAsync(accountId)
                      ?? throw new NotFoundException("Holiday account not found.");

        // Calculate actual balance, excluding Used transactions.
        var actualBalance = await CalculateBalance(
            account,
            checkpoint,
            includeUsedTransactions: false
        );

        // Add earnings that have not yet been recorded.
        var projectedEarnings = await earningService.CalculateProjectedEarnings(
            account.UserId,
            accountId,
            checkpoint
        );

        return decimal.Round(actualBalance + projectedEarnings, 2);
    }
    
    public async Task<decimal> GetUserProjectedBalance(
        string userId,
        DateOnly checkpoint)
    {
        var accounts = await accountRepo.GetUserAccountsAsync(userId);

        decimal totalBalance = 0;

        foreach (var account in accounts)
        {
            totalBalance += await GetProjectedBalance(
                account.Id,
                checkpoint
            );
        }

        return decimal.Round(totalBalance, 2);
    }
}
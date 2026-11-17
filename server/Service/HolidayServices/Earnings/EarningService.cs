
using System.ComponentModel.DataAnnotations;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Repositories.Interfaces;

namespace Service.HolidayServices.Earnings;

public class EarningService(
    EarningCalculator earningCalculator,
    IEarningRepo earningRepo,
    IAccountRepo accountRepo,
    IUserRepo userRepo,
    IWorkScheduleRepo workScheduleRepo
)
{
    public async Task ProcessMonthlyEarningsForUser(
        string userId,
        DateOnly monthStart,
        DateOnly monthEnd)
    {
        if (monthStart.Day != 1)
        {
            throw new ArgumentException(
                "MonthStart must be the first day of the month."
            );
        }

        var accounts = await accountRepo.GetUserAccountsAsync(userId);

        var week1_5 = accounts.SingleOrDefault(
            a => a.Type == HolidayType.Week1_5
        ) ?? throw new ValidationException("Week1_5 account not found");

        var week6 = accounts.SingleOrDefault(
            a => a.Type == HolidayType.Week6
        ) ?? throw new ValidationException("Week6 account not found");

        var schedules = await workScheduleRepo.GetHistoryForUser(userId);

        if (schedules.Count == 0)
        {
            throw new ValidationException(
                "No work schedule history found"
            );
        }

        var result = earningCalculator.CalculateMonthlyEarnings(
            schedules,
            monthStart,
            monthEnd
        );
        
        var earningsToSave = new List<HolidayEarning>();

        // Week 1-5
        if (!await earningRepo.EarningsExist(week1_5.Id, monthStart))
        {
            earningsToSave.Add(new HolidayEarning
            {
                Id = Guid.NewGuid(),
                HolidayAccountId = week1_5.Id,
                EarnedDate = monthStart,
                EarnedDays = result.Week1_5Days,
                EarnedHours = result.Week1_5Hours,
                ExpiryDate = CalculateWeek1_5ExpiryDate(monthStart)
            });
        }

        // Week 6
        if (!await earningRepo.EarningsExist(week6.Id, monthStart))
        {
            earningsToSave.Add(new HolidayEarning
            {
                Id = Guid.NewGuid(),
                HolidayAccountId = week6.Id,
                EarnedDate = monthStart,
                EarnedDays = result.Week6Days,
                EarnedHours = result.Week6Hours,
                ExpiryDate = CalculateWeek6ExpiryDate(monthStart)
            });
        }

        // Save all missing earnings together.
        if (earningsToSave.Count > 0)
        {
            await earningRepo.SaveEarningsTransaction(earningsToSave);
        }

    }
    
    /**
     * Finds all users, finds the first workschedule for the user
     * and starts controlling if there are already saved earnings from that month.
     */
    public async Task ProcessMonthlyEarnings(
        CancellationToken cancellationToken = default)
    {
        var users = await userRepo.GetAll();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // The current month is not completed yet.
        var lastCompletedMonth = new DateOnly(
            today.Year,
            today.Month,
            1
        ).AddMonths(-1);

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var schedules = await workScheduleRepo.GetHistoryForUser(
                user.UserId
            );

            if (schedules.Count == 0)
                continue;

            // Start with the month of the user's first work schedule.
            var firstScheduleDate = schedules.Min(s => s.EffectiveFrom);

            var monthStart = new DateOnly(
                firstScheduleDate.Year,
                firstScheduleDate.Month,
                1
            );

            while (monthStart <= lastCompletedMonth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                await ProcessMonthlyEarningsForUser(
                    user.UserId,
                    monthStart,
                    monthEnd
                );

                monthStart = monthStart.AddMonths(1);
            }
        }
    }


    private DateOnly CalculateWeek1_5ExpiryDate(DateOnly earnedDate)
    {
        int expiryYear = earnedDate.Month >= 9
            ? earnedDate.Year + 2
            : earnedDate.Year + 1;

        return new DateOnly(expiryYear, 12, 31);
    }

    private DateOnly CalculateWeek6ExpiryDate(DateOnly earnedDate)
    {
        int expiryYear = earnedDate.Year + 2;

        return new DateOnly(expiryYear, 4, 30);
    }
}

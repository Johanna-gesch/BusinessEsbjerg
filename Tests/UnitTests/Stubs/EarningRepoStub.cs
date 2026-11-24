using Infra.Entities.HolidayTracking.Earnings;
using Infra.Repositories.Interfaces;

namespace Tests.UnitTests.Stubs;

public class EarningRepoStub : IEarningRepo
{
    private readonly List<HolidayEarning> _earnings = new();

    public void AddEarning(HolidayEarning earning)
    {
        _earnings.Add(earning);
    }
    
    public Task SaveEarningsTransaction(List<HolidayEarning> earnings)
    {
        _earnings.AddRange(earnings);
        return Task.CompletedTask;
    }

    public Task<List<HolidayEarning>> GetEarningsByUserAndAccount(
        string userId,
        Guid accountId)
    {
        var earnings = _earnings
            .Where(e => e.UserId == userId &&
                        e.HolidayAccountId == accountId)
            .ToList();

        return Task.FromResult(earnings);
    }

    public Task<bool> EarningsExist(Guid accountId, DateOnly earnedDate)
    {
        var exists = _earnings.Any(e =>
            e.HolidayAccountId == accountId &&
            e.EarnedDate == earnedDate);

        return Task.FromResult(exists);
    }
}
using Infra.Entities.HolidayTracking.Earnings;

namespace Infra.Repositories.Interfaces;

public interface IEarningRepo
{
    Task SaveEarnings(HolidayEarning earning);

}
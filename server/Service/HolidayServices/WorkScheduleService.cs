using Infra;
using Infra.Repositories.Interfaces;
using Service.DTOs.WorkScheduleDTOs;

namespace Service.HolidayServices;

public class WorkScheduleService (IWorkScheduleRepo repo)
{
    public async Task<WorkScheduleDto> GetEffective(string userId, DateOnly date)
    {
        var current = await repo.GetCurrent(userId);
        if (current != null && current.EffectiveFrom <= date)
        {
            return new WorkScheduleDto
            {
                EffectiveFrom = current.EffectiveFrom,
                Monday = current.MondayHours,
                Tuesday = current.TuesdayHours,
                Wednesday = current.WednesdayHours,
                Thursday = current.ThursdayHours,
                Friday = current.FridayHours,
            };
        }

        var history = await repo.GetHistoryEffective(userId, date)
                      ?? throw new NotFoundException($"No work schedule for user {userId} on {date}");

        return new WorkScheduleDto
        {
            EffectiveFrom = history.EffectiveFrom,
            Monday = history.MondayHours,
            Tuesday = history.TuesdayHours,
            Wednesday = history.WednesdayHours,
            Thursday = history.ThursdayHours,
            Friday = history.FridayHours,
        };
    }
    
    
}
using Infra;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;
using Service.DTOs.WorkScheduleDTOs;

namespace Service.HolidayServices;

public class WorkScheduleService (IWorkScheduleRepo repo, IUserRepo userRepo)
{
    /*
     * This method is for retrieving the work schedule for individual dates.
     */
    public async Task<WorkScheduleDto> GetEffective(string userId, DateOnly date)
    {
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

    /*
     * This method is designed to get schedules relevant to a period of time.
     */
    public async Task<List<WorkScheduleDto>> GetSchedulesForPeriod(
        string userId,
        DateOnly start,
        DateOnly end)
    {
        if (start > end)
            throw new ValidationException("Start date can't be after the end date.");

        var schedules = await repo.GetSchedulesForPeriod(userId, start, end);

        return schedules.Select(s => new WorkScheduleDto
        {
            EffectiveFrom = s.EffectiveFrom,
            Monday = s.MondayHours,
            Tuesday = s.TuesdayHours,
            Wednesday = s.WednesdayHours,
            Thursday = s.ThursdayHours,
            Friday = s.FridayHours,
        }).ToList();
    }

    /*
     * This method retrieves the schedules for a period of time.
     * It then uses HolidayCalculator to calculate the required hours.
     */
    public async Task<decimal> CalculateRequiredHours(
        string userId,
        DateOnly start,
        DateOnly end,
        IReadOnlySet<DateOnly> publicHolidays)
    {
        var schedules = await GetSchedulesForPeriod(userId, start, end);

        return HolidayCalculator.CalculateHours(start, end, date =>
            {
                var schedule = schedules
                                   .Where(s => s.EffectiveFrom <= date)
                                   .OrderByDescending(s => s.EffectiveFrom)
                                   .FirstOrDefault()
                               ?? throw new NotFoundException($"No work schedule exists for {date}.");
                
                return date.DayOfWeek switch
                {
                    DayOfWeek.Monday => schedule.Monday,
                    DayOfWeek.Tuesday => schedule.Tuesday,
                    DayOfWeek.Wednesday => schedule.Wednesday,
                    DayOfWeek.Thursday => schedule.Thursday,
                    DayOfWeek.Friday => schedule.Friday,
                    _ => 0
                };
            },
            publicHolidays);
    }

    // Validates the request and saves a new work schedule in the history
    public async Task<WorkScheduleDto> Create(CreateWorkScheduleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ValidationException("User id is required");
        if (dto.EffectiveFrom == default)
            throw new ValidationException("EffectiveFrom is required");
        
        var hours = new []{
            dto.Monday, dto.Tuesday, dto.Wednesday, dto.Thursday, dto.Friday};
        if (hours.Any(h => h < 0 || h > 24))
            throw new ValidationException("Hours per day must be between 0 and 24");
        if (hours.All(h => h == 0))
            throw new ValidationException ("At least one day must have hours");

        if (await userRepo.GetById(dto.UserId) is null)
            throw new NotFoundException($"User {dto.UserId} not found");

        // A user can only have one work schedule per start date.
        var existing = await repo.GetHistoryForUser(dto.UserId);
        if (existing.Any(s => s.EffectiveFrom == dto.EffectiveFrom))
            throw new ConflictException($"User already has a work schedule from {dto.EffectiveFrom}");

        
        // Map the validated request to the entity that is stored in the db
        var entity = new WorkScheduleHistory
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            EffectiveFrom = dto.EffectiveFrom,
            MondayHours = dto.Monday,
            TuesdayHours = dto.Tuesday,
            WednesdayHours = dto.Wednesday,
            ThursdayHours = dto.Thursday,
            FridayHours = dto.Friday
        };

        await repo.Insert(entity);

        // return a DTO instead of exposing the database entity to the caller.
        return new WorkScheduleDto
        {
            EffectiveFrom = entity.EffectiveFrom,
            Monday = entity.MondayHours,
            Tuesday = entity.TuesdayHours,
            Wednesday = entity.WednesdayHours,
            Thursday = entity.ThursdayHours,
            Friday = entity.FridayHours
        };
    }
    
    
    
    
    /*
     * Retrieves the effective work schedule for a date
     * and calculates the employee's total weekly hours.
     */
    public async Task<decimal> GetWeeklyHours(string userId, DateOnly date)
    {
        var schedule = await GetEffective(userId, date);

        return schedule.Monday
               + schedule.Tuesday
               + schedule.Wednesday
               + schedule.Thursday
               + schedule.Friday;
    }
}
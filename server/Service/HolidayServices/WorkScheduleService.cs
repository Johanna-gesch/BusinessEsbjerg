using Infra;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Infra.Repositories.Interfaces;
using Service.DTOs.WorkScheduleDTOs;

namespace Service.HolidayServices;

public class WorkScheduleService (IWorkScheduleRepo repo, IUserRepo userRepo)
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
    
    
    
    
}
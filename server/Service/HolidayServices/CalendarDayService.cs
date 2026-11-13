using Infra;
using Infra.Entities.HolidayTracking;
using Infra.Repositories.Interfaces;
using Service.DTOs.CalendarDayDTOs;

namespace Service.HolidayServices;

public class CalendarDayService (ICalendarDayRepo repo)
{
    public async Task<CalendarDayDTO> Create(CreateCalendarDayDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Name is required");

        if (await repo.GetByDate(dto.Date) != null)
            throw new ConflictException($"{dto.Date} is already a calendar day");

        var entity = new CalendarDay
        {
            Id = Guid.NewGuid(),
            Date = dto.Date,
            Name = dto.Name
        };
        await repo.Insert(entity);

        return new CalendarDayDTO
        {
            Id = entity.Id,
            Date = entity.Date,
            Name = entity.Name
        };
    }
    
}
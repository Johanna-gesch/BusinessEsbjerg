namespace Service.DTOs.CalendarDayDTOs;

public class CreateCalendarDayDTO
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = "";
}
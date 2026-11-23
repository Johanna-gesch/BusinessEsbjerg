namespace Service.DTOs.SharedCalendarDTos;

public class SharedCalendarDayDto
{
    public DateOnly Date { get; set; }
    public SharedCalendarDayStatus Status { get; set; }
}
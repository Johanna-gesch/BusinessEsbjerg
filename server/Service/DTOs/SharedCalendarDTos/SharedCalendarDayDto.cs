namespace Service.DTOs.SharedCalendarDTos;

/*
 * The status of one day for one employee.
 */
public class SharedCalendarDayDto
{
    public DateOnly Date { get; set; }
    public SharedCalendarDayStatus Status { get; set; }
}
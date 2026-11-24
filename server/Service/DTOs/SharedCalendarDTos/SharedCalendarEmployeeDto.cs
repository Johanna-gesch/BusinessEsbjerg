namespace Service.DTOs.SharedCalendarDTos;

/*
 * One employee in the shared calendar, with a status for every day in the period.
 */
public class SharedCalendarEmployeeDto
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public List<SharedCalendarDayDto> Days { get; set; } = new();
}
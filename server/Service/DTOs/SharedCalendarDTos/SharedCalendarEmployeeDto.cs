namespace Service.DTOs.SharedCalendarDTos;

public class SharedCalendarEmployeeDto
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public List<SharedCalendarDayDto> Days { get; set; } = new();
}
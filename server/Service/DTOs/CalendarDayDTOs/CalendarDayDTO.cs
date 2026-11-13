namespace Service.DTOs.CalendarDayDTOs;

public class CalendarDayDTO
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Name { get; set; } = "";
    
}
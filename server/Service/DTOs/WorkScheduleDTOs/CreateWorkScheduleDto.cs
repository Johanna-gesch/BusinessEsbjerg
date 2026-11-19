namespace Service.DTOs.WorkScheduleDTOs;

public class CreateWorkScheduleDto
{
    public string UserId { get; set; } = "";
    public DateOnly EffectiveFrom { get; set; }

    public decimal Monday { get; set; } = 7.5m;
    public decimal Tuesday { get; set; } = 7.5m;
    public decimal Wednesday { get; set; } = 7.5m;
    public decimal Thursday { get; set; } = 7.5m;
    public decimal Friday { get; set; } = 7m;

    
}
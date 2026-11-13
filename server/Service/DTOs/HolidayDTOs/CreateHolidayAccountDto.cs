using Infra.Entities;

namespace Service.DTOs.HolidayDTOs;

public class CreateHolidayAccountDto
{
    public string UserId { get; set; } = "";
    public HolidayType Type { get; set; }
}
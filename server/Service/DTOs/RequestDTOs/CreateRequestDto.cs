namespace Service.DTOs.RequestDTOs;

public class CreateRequestDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
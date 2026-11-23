namespace Service.DTOs.RequestDTOs;

public class CreateRequestDto
{
    //public Guid HolidayAccountId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
namespace Service.DTOs.HolidayDTOs;

/**
 * This DTO is used for getting a user's remaining available hours. 
 */
public class UserHolidayBalanceDto
{
    public decimal Week1_5Hours { get; set; }
    public decimal Week6Hours { get; set; }

    public decimal TotalHours => Week1_5Hours + Week6Hours;
}
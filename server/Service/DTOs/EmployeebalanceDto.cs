using LinqToDB.Mapping;

namespace Service.DTOs;

public class EmployeebalanceDto
{

    [NotNull] public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public decimal Week1_5Hours { get; set; }
    public decimal Week6Hours { get; set; }

    public decimal TotalHours => Week1_5Hours + Week6Hours;
}
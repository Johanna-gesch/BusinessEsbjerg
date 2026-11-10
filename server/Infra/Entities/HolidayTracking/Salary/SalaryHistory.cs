using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.Salary;

[Table("SalaryHistory")]
public class SalaryHistory
{
    [PrimaryKey] public Guid Id { get; set; }
    [Column] public string UserId { get; set; } = "";
    
    [Column] public DateOnly EffectiveFrom { get; set; }
    [Column] public decimal MonthlySalary { get; set; }
}

using LinqToDB.Mapping;

namespace Infra.Entities;

public class HolidayAccount
{
    [Column] public HolidayType type { get; set; } = HolidayType.Employee;
}
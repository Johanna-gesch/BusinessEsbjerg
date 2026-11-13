using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking;

[Table("CalendarDay")]
public class CalendarDay
{
    [PrimaryKey] public Guid Id { get; set;}
    [Column] public DateOnly Date { get; set;}
    [Column] public string Name { get; set;}

}
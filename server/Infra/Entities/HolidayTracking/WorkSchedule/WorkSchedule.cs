using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.WorkSchedule;

[Table("WorkSchedule")]
public class WorkSchedule
{
    [PrimaryKey] public Guid Id { get; set; }
    [Column] public string UserId { get; set; } = "";
    
    [Column] public decimal MondayHours { get; set; }
    [Column] public decimal TuesdayHours { get; set; }
    [Column] public decimal WednesdayHours { get; set; }
    [Column] public decimal ThursdayHours { get; set; }
    [Column] public decimal FridayHours { get; set; }
}
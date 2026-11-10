using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.Earnings;

[Table("HolidayEarning")]
public class HolidayEarning
{
    [PrimaryKey] public Guid Id { get; set; }
    [Column] public Guid HolidayAccountId { get; set; }
    
    [Column] public DateOnly EarnedDate { get; set; }
    [Column] public decimal EarnedHours { get; set; }
    [Column] public DateOnly ExpiryDate { get; set; }
}

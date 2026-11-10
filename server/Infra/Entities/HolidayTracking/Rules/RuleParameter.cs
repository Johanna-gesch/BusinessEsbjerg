using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.Rules;

[Table("RuleParameter")]
public class RuleParameter
{
    [PrimaryKey] public Guid Id { get; set; }

    [Column] public string Key { get; set; } = "";
    [Column] public decimal Value { get; set; }
    [Column] public DateOnly EffectiveFrom { get; set; }
    [Column] public DateOnly? EffectiveTo { get; set; }
}
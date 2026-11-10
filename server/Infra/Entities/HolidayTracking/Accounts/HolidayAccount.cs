using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Transactions;
using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.Accounts;

[Table("HolidayAccount")]
public class HolidayAccount
{
    [PrimaryKey] public Guid Id { get; set; }
    [Column] public string UserId { get; set; } = "";
    
    [Column] public HolidayType Type { get; set; } = HolidayType.Week1_5;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(HolidayEarning.HolidayAccountId))]
    public ICollection<HolidayEarning> Earnings { get; set; } = new List<HolidayEarning>();

    [Association(ThisKey = nameof(Id), OtherKey = nameof(HolidayTransaction.HolidayAccountId))]
    public ICollection<HolidayTransaction> Transactions { get; set; } = new List<HolidayTransaction>();
}
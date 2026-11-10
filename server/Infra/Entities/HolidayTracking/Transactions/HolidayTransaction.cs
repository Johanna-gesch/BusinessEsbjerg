using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.Transactions;

[Table("HolidayTransaction")]
public class HolidayTransaction
{
    [PrimaryKey] public Guid Id { get; set; }
    [Column] public Guid HolidayAccountId { get; set; }
    [Column] public Guid? HolidayEarningId { get; set; }
    [Column] public decimal AmountHours { get; set; }
    [Column] public DateOnly TransactionDate { get; set; }
    [Column] public HolidayTransactionType TransactionType { get; set; }
    [Column] public string? Description { get; set; }
}

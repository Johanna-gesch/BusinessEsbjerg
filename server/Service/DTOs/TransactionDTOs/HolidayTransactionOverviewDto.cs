using Infra.Entities.HolidayTracking.Transactions;

namespace Service.DTOs.TransactionDTOs;

/**
 * This DTO leaves out account and earning ID, since a combined
 * transaction can involve multiple earnings and accounts.
 */
public class HolidayTransactionOverviewDto
{
    public Guid Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public HolidayTransactionType TransactionType { get; set; }
    public string? Description { get; set; }
    public decimal AmountHours { get; set; }
}
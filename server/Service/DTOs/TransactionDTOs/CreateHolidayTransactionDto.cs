using Infra.Entities.HolidayTracking.Transactions;

namespace Service.DTOs.TransactionDTOs;

public class CreateHolidayTransactionDto
{
    public Guid HolidayAccountId { get; set; }
    public Guid? HolidayEarningId { get; set; }
    public decimal AmountHours { get; set; }
    public DateOnly TransactionDate { get; set; }
    public HolidayTransactionType TransactionType { get; set; }
    public string? Description { get; set; }
    
}
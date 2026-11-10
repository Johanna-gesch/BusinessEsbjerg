using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Requests;
using LinqToDB.Mapping;

namespace Infra.Entities.HolidayTracking.Requests;

[Table("HolidayRequest")]
public class HolidayRequest
{
    [PrimaryKey] public Guid Id { get; set; }
    [Column] public string UserId { get; set; } = "";
    [Column] public Guid HolidayAccountId { get; set; }
    
    [Column] public DateTime CreatedAt { get; set; }
    [Column] public string? CancellationReason { get; set; }
    
    [Column] public DateOnly StartDate { get; set; }
    [Column] public DateOnly EndDate { get; set; }
    
    [Column] public HolidayRequestStatus Status { get; set; }
    
    [Column] public string? ApprovedByUserId { get; set; }
    [Column] public DateTime? ApprovedAt { get; set; }
    [Column] public string? RejectionReason { get; set; }

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId))]
    public User User { get; set; } = null!;

    [Association(ThisKey = nameof(HolidayAccountId), OtherKey = nameof(HolidayAccount.Id))]
    public HolidayAccount HolidayAccount { get; set; } = null!;
}
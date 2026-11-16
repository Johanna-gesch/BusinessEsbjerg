using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.Salary;
using LinqToDB.Mapping;

namespace Infra.Entities;

[Table("User")]
public class User
{
    [PrimaryKey] public string UserId { get; set; } = "";
    [Column] public string Username { get; set; } = "";
    [Column] public string PasswordHash { get; set; } = "";
    [Column] public bool IsAdmin { get; set; } = false;
    [Column] public bool IsActive { get; set; } = true;
    
    [Column] public SalaryType SalaryType { get; set; } = SalaryType.Monthly;

    [Column] public decimal Salary { get; set; } = 0;

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(HolidayAccount.UserId))]
    public ICollection<HolidayAccount> HolidayAccounts { get; set; } = new List<HolidayAccount>();

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(HolidayRequest.UserId))]
    public ICollection<HolidayRequest> HolidayRequests { get; set; } = new List<HolidayRequest>();
}
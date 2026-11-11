using LinqToDB;
using LinqToDB.Data;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Accounts;
using Infra.Entities.HolidayTracking.Earnings;
using Infra.Entities.HolidayTracking.Requests;
using Infra.Entities.HolidayTracking.Rules;
using Infra.Entities.HolidayTracking.Salary;
using Infra.Entities.HolidayTracking.Transactions;
using Infra.Entities.HolidayTracking.WorkSchedule;

namespace Infra;

public class MyDatabaseConnection(DataOptions<MyDatabaseConnection> options) : DataConnection(options.Options)
{
    public ITable<User> Users => this.GetTable<User>();

    public ITable<HolidayAccount> HolidayAccounts => this.GetTable<HolidayAccount>();

    public ITable<HolidayEarning> HolidayEarnings => this.GetTable<HolidayEarning>();

    public ITable<HolidayTransaction> HolidayTransactions => this.GetTable<HolidayTransaction>();

    public ITable<HolidayRequest> HolidayRequests => this.GetTable<HolidayRequest>();

    public ITable<WorkSchedule> WorkSchedules => this.GetTable<WorkSchedule>();
    public ITable<WorkScheduleHistory> WorkScheduleHistories => this.GetTable<WorkScheduleHistory>();

    public ITable<SalaryHistory> SalaryHistories => this.GetTable<SalaryHistory>();

    public ITable<RuleParameter> RuleParameters => this.GetTable<RuleParameter>();
}
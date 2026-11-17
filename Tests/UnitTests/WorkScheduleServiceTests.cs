using Infra;
using Infra.Entities.HolidayTracking.WorkSchedule;
using Service.HolidayServices;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class WorkScheduleServiceTests
{
    private readonly WorkScheduleRepoStub _repoStub = new WorkScheduleRepoStub();
    private readonly WorkScheduleService _service;

    public WorkScheduleServiceTests()
    {
        _service = new WorkScheduleService(_repoStub);
        
        //current workweek from 1. sept 2026
        _repoStub.Current.Add(new WorkSchedule
        {
            Id = Guid.NewGuid(),UserId = "Malthe", EffectiveFrom = new DateOnly(2026, 9, 1),
            MondayHours = 7.5m, TuesdayHours = 7.5m, WednesdayHours = 7.5m,
            ThursdayHours = 7.5m, FridayHours = 7m
        });
        
        // previous workweek from 1.jan 2025
        _repoStub.History.Add(new WorkScheduleHistory
        {
            Id = Guid.NewGuid(), UserId = "Malthe", EffectiveFrom = new DateOnly(2025, 1, 1),
            MondayHours = 6m, TuesdayHours = 6m, WednesdayHours = 6m,
            ThursdayHours = 6m, FridayHours = 6m

        });
    }

    [Fact]
    public async Task GetEffective_DateAfterCurrentStarts_ReturnsCurrent()
    {
        var result = await _service.GetEffective("Malthe", new DateOnly(2026, 10, 12));
        Assert.Equal(7.5m, result.Monday);
        Assert.Equal(7m, result.Friday);
    }

    [Fact]
    public async Task GetEffective_DateBeforeCurrentStarts_ReturnsHistory()
    {
        var result = await _service.GetEffective("Malthe", new DateOnly(2026, 3, 10));
        
        Assert.Equal(6m, result.Monday);
    }

    [Fact]
    public async Task GetEffective_DateBeforeAnySchedule_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetEffective("Malthe", new DateOnly(2024, 12, 5)));
    }
    
    [Fact]
    public async Task HoursFor_GivesHoursForWeekdaysAndZeroForWeekend()
    {
        var result = await _service.GetEffective("Malthe", new DateOnly(2026, 10, 12));

        Assert.Equal(7.5m, result.HoursFor(DayOfWeek.Thursday));
        Assert.Equal(7m, result.HoursFor(DayOfWeek.Friday));
        Assert.Equal(0m, result.HoursFor(DayOfWeek.Saturday));
    }
}
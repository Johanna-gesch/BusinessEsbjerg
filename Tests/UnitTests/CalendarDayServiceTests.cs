using Infra;
using Service.DTOs.CalendarDayDTOs;
using Service.HolidayServices;
using Tests.UnitTests.Stubs;

namespace Tests.UnitTests;

public class CalendarDayServiceTests
{
    private readonly CalendarDayRepoStub _repoStub = new();
    private readonly CalendarDayService _service;
    
    public CalendarDayServiceTests()
    {
        _service = new CalendarDayService(_repoStub);
    }

    [Fact]
    public async Task Create_EmptyName_ThrowsValidationException()
    {
        var dto = new CreateCalendarDayDTO { Date = new DateOnly(2026, 12, 24), Name = "" };
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(dto));
    }

    [Fact]
    public async Task Create_DuplicateDate_ThrowsConflictException()
    {
        var dto = new CreateCalendarDayDTO { Date = new DateOnly(2026, 12, 24), Name = "Juleaften" };
        await _service.Create(dto);
        await Assert.ThrowsAsync<ConflictException>(() => _service.Create(dto));
    }
    
    [Fact]
    public async Task Create_ValidInput_SavesAndReturnsDto()
    {
        var dto = new CreateCalendarDayDTO { Date = new DateOnly(2026, 12, 24), Name = "Juleaften" };
        var result = await _service.Create(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(_repoStub.Days);
    }

    [Fact]
    public async Task GetHolidayDates_ReturnsOnlyDatesInsideThePEriod()
    {
        await _service.Create(new CreateCalendarDayDTO { Date = new DateOnly(2026, 10, 15), Name = "A" });
        await _service.Create(new CreateCalendarDayDTO { Date = new DateOnly(2026, 10, 15), Name = "New Year" });
        
        var result = await _service.GetHolidayDates(new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 16));
        
        Assert.Single(result);
        Assert.Contains(new DateOnly(2026, 10, 15), result);

    }
    

}
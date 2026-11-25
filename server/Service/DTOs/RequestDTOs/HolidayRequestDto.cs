using Infra.Entities.HolidayTracking.Requests;

namespace Service.DTOs.RequestDTOs;

public class HolidayRequestDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public HolidayRequestStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
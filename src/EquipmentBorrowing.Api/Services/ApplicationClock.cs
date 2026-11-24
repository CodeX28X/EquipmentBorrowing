namespace EquipmentBorrowing.Api.Services;
public sealed class ApplicationClock(TimeProvider timeProvider)
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Singapore Standard Time" : "Asia/Manila");
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), _timeZone).DateTime);
}

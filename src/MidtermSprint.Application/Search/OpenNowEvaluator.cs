using MidtermSprint.Domain.Entities;

namespace MidtermSprint.Application.Search;

/// <summary>
/// Evaluates restaurant hours in the Asia/Manila time zone, including overnight opening hours.
/// </summary>
public sealed class OpenNowEvaluator(TimeZoneInfo? timeZone = null)
{
    private readonly TimeZoneInfo zone =
        timeZone ?? TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    public bool IsOpenNow(Restaurant restaurant, DateTimeOffset utcNow)
    {
        var localNow = TimeZoneInfo.ConvertTime(utcNow, zone);
        var localTime = TimeOnly.FromDateTime(localNow.DateTime);
        var today = restaurant.Hours.FirstOrDefault(hour => hour.DayOfWeek == localNow.DayOfWeek);
        if (IsOpenDuring(today, localTime))
        {
            return true;
        }

        var yesterday = restaurant.Hours.FirstOrDefault(hour =>
            hour.DayOfWeek == localNow.AddDays(-1).DayOfWeek);
        return IsOvernightCarry(yesterday, localTime);
    }

    private static bool IsOpenDuring(RestaurantHour? hours, TimeOnly time)
    {
        if (hours is null || hours.IsClosed || hours.OpensAt is not { } opensAt ||
            hours.ClosesAt is not { } closesAt || opensAt == closesAt)
        {
            return false;
        }

        return opensAt < closesAt
            ? time >= opensAt && time < closesAt
            : time >= opensAt;
    }

    private static bool IsOvernightCarry(RestaurantHour? hours, TimeOnly time) =>
        hours is { IsClosed: false, OpensAt: { } opensAt, ClosesAt: { } closesAt } &&
        opensAt > closesAt &&
        time < closesAt;
}

namespace Kisse.Models;

/// <summary>
/// Data model for index page, containing markers to display on the map.
/// </summary>
public record IndexMapModel
{
    // id -> lat, lng
    public required IDictionary<int, ValueTuple<double, double>> Cats;
    public required IDictionary<int, ValueTuple<double, double>> Observations;
}

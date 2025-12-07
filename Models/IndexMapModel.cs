namespace Kisse.Models;

/**
 * Data model for index page, containing markers to display on the map.
 */
public record IndexMapModel
{
    // id -> lat, lng
    public required IDictionary<int, ValueTuple<double, double>> Cats;
    public required IDictionary<int, ValueTuple<double, double>> Observations;
}

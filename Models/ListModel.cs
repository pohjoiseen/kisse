namespace Kisse.Models;

/// <summary>
/// Generic model to display paginated data.
/// </summary>
/// <typeparam name="T">Any model</typeparam>
public record ListModel<T>
{
    public required IList<T> Data { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalData { get; set; }
}
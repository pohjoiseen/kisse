namespace Kisse.Models;

public record ListModel<T>
{
    public required IList<T> Data { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalData { get; set; }
}
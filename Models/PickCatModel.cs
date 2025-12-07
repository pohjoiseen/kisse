using Kisse.Data;

namespace Kisse.Models;

public record PickCatModel
{
    public int? Id { get; set; }
    public CatModel? Cat { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public IList<Photo>? Photos { get; set; }
    public required IList<CatModel> NearestCats { get; set; }
}
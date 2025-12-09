using Microsoft.EntityFrameworkCore;

namespace Kisse.Data;

/// <summary>
/// Single uploaded cat photo.  Linked to an Observation (except before the Observation is saved;
/// photo can get orphaned if page is abandoned before saving).  Saves paths to uploaded image and its
/// thumbnail version, and extracted metadata (photo date/time and GPS coordinates in particular).
/// </summary>
[Index(nameof(ObservationId))]
public class Photo
{
    public int Id { get; init; }
    public int? ObservationId { get; set; }
    public Observation? Observation { get; set; }
    public DateTime Date { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public required string OriginalFile { get; set; }
    public required string ThumbnailFile { get; set; }

    /// <summary>
    /// Delete uploaded files for this photo, and the folder where they were stored.
    /// </summary>
    /// <param name="configuration">App configuration, to look for correct paths.</param>
    public void DeleteFiles(IConfiguration configuration)
    {
        File.Delete(ThumbnailFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]));
        File.Delete(OriginalFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]));
        Directory.Delete(Path.GetDirectoryName(ThumbnailFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]))!);
    }
}
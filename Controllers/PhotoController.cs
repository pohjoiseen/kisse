using System.Globalization;
using System.Security.Claims;
using Kisse.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace Kisse.Controllers;

[Authorize]
public class PhotoController(IConfiguration configuration, ApplicationDbContext dbContext) : Controller
{
    public static readonly int ThumbnailSize = 200;

    public async Task<IActionResult> View(int id)
    {
        var photo = await dbContext.Photos
            .Include(p => p.Observation)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (photo is null)
        {
            return NotFound();
        }
        
        return View(photo);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var photo = await dbContext.Photos
            .Include(p => p.Observation)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (photo is null)
        {
            return NotFound();
        }
        if (photo.Observation is not null && photo.Observation.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Forbid();
        }

        dbContext.Photos.Remove(photo);
        try
        {
            System.IO.File.Delete(photo.ThumbnailFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]));
            System.IO.File.Delete(photo.OriginalFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]));
            Directory.Delete(Path.GetDirectoryName(photo.ThumbnailFile.Replace(configuration["Upload:URL"]!, configuration["Upload:Path"]))!);
        }
        catch
        {
            // ignore possible errors
        }
        await dbContext.SaveChangesAsync();
        
        return View(photo);
    }
        
    public async Task<IActionResult> Upload(IFormFileCollection? files)
    {
        if (files is null)
        {
            return BadRequest();
        }

        var photos = new List<Photo>();
        foreach (var file in files)
        {
            if (file.ContentType != "image/jpeg")
            {
                continue;
            }

            // save photo to a random UUID subdir
            var filename = Path.GetFileName(file.FileName).Replace(' ', '_');
            var uuid = Guid.NewGuid().ToString("N");
            var dir = configuration["Upload:Path"] + uuid;
            var path = Path.Combine(dir, filename);
            Directory.CreateDirectory(dir);
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            await using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                await fs.WriteAsync(ms.GetBuffer());
            }

            // load for processing
            ms.Seek(0, SeekOrigin.Begin);
            var image = await Image.LoadAsync(ms);

            // determine thumbnail size (shortest side must match ThumbnailSize) and save thumbnail 
            int thumbWidth, thumbHeight;
            if (image.Width > image.Height)
            {
                thumbHeight = ThumbnailSize;
                thumbWidth = image.Width * ThumbnailSize / image.Height;
            }
            else
            {
                thumbWidth = ThumbnailSize;
                thumbHeight = image.Height * ThumbnailSize / image.Width;
            }

            var thumbpath = Path.Combine(dir, "t-" + filename);
            using (var thumbnail = image.Clone(x => x.Resize(thumbWidth, thumbHeight)))
            {
                await thumbnail.SaveAsJpegAsync(thumbpath, new JpegEncoder { Quality = 75 });
            }

            // find original datetime from EXIF, if any, default to now
            DateTime date = DateTime.UtcNow;
            var originalDateTimeString = (string?)image.Metadata.ExifProfile?.Values
                .FirstOrDefault(v => v.Tag == ExifTag.DateTimeOriginal)?.GetValue();
            if (originalDateTimeString != null && originalDateTimeString != "0000:00:00 00:00:00")
            {
                DateTime.TryParseExact(originalDateTimeString, "yyyy:MM:dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date);
                date = date.ToUniversalTime();
            }

            // find coordinates from EXIF, if any, default to zeroes
            double? lat = null, lng = null;
            if (image.Metadata.ExifProfile?.TryGetValue(ExifTag.GPSLatitude,
                    out IExifValue<Rational[]>? latitudeParts) == true && latitudeParts.Value?.Length == 3)
            {
                uint degrees = latitudeParts.Value[0].Numerator;
                double minutes = latitudeParts.Value[1].Numerator / 60D;
                double seconds = (latitudeParts.Value[2].Numerator / (double)latitudeParts.Value[2].Denominator) /
                                 3600D;
                lat = degrees + minutes + seconds;
            }

            if (image.Metadata.ExifProfile?.TryGetValue(ExifTag.GPSLongitude,
                    out IExifValue<Rational[]>? longitudeParts) == true && longitudeParts.Value?.Length == 3)
            {
                uint degrees = longitudeParts.Value[0].Numerator;
                double minutes = longitudeParts.Value[1].Numerator / 60D;
                double seconds = (longitudeParts.Value[2].Numerator / (double)longitudeParts.Value[2].Denominator) /
                                 3600D;
                lng = degrees + minutes + seconds;
            }

            var photo = new Photo
            {
                Date = date,
                Lat = lat,
                Lng = lng,
                Width = image.Width,
                Height = image.Height,
                OriginalFile = Path.Combine(configuration["Upload:URL"]!, uuid, filename),
                ThumbnailFile = Path.Combine(configuration["Upload:URL"]!, uuid, "t-" + filename)
            };
            dbContext.Photos.Add(photo);
            photos.Add(photo);
        }

        await dbContext.SaveChangesAsync();
            
        return View(photos);
    }
}
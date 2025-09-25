using webapp.src;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS (allow all origins for now; change in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Relative paths (cloud-friendly)
string rootPath = Directory.GetCurrentDirectory();
string coversPath = Path.Combine(rootPath, "Mangafiles", "Covers");
string allMangaPath = Path.Combine(rootPath, "Mangafiles", "allmanga");
string detailsJson = Path.Combine(rootPath, "Mangafiles", "details.json");

// Enable CORS
app.UseCors("AllowReactApp");

// Serve static cover images
if (Directory.Exists(coversPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(coversPath),
        RequestPath = "/covers"
    });
}

// Serve static manga images (chapter pages)
if (Directory.Exists(allMangaPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(allMangaPath),
        RequestPath = "/manga"
    });
}

// Root endpoint
app.MapGet("/", () => "Hello World!");

// Cover service
var coverService = new CoverService(detailsJson);

/// Get all covers / manga details
app.MapGet("/covers", () =>
{
    return coverService.GetCovers();
});

/// Get manga names only
app.MapGet("/names", () =>
{
    return coverService.GetCovers().Select(c => c.Name);
});

/// Get names + URLs (for React)
app.MapGet("/names-url", () =>
{
    return coverService.GetCovers().Select(c => new
    {
        c.Name,
        c.Url
    });
});

/// Get list of pages for a specific manga chapter
app.MapGet("/manga/{mangaName}/{chapterId}/pages", (string mangaName, string chapterId) =>
{
    var chapterPath = Path.Combine(allMangaPath, mangaName, chapterId);

    if (!Directory.Exists(chapterPath))
        return Results.NotFound("Chapter folder not found");

    var pngFiles = Directory.GetFiles(chapterPath, "*.png")
        .Select(f => Path.GetFileName(f))
        .OrderBy(f =>
        {
            var numPart = Path.GetFileNameWithoutExtension(f);
            return int.TryParse(numPart, out int n) ? n : 0;
        })
        .ToList();

    return Results.Json(pngFiles);
});

app.Run();

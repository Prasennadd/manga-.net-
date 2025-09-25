using webapp.src;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Enable CORS for React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173", "http://192.168.29.155:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Paths
string coversPath = @"C:\Mangafiles\Covers";
string allMangaPath = @"C:\Mangafiles\allmanga"; // <- new: folder containing all manga chapters
string detailsJson = @"C:\Mangafiles\details.json";

// Enable CORS
app.UseCors("AllowReactApp");

// Serve static cover images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(coversPath),
    RequestPath = "/covers"
});

// Serve static manga images (chapter pages)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(allMangaPath),
    RequestPath = "/manga"
});

// Root endpoint
app.MapGet("/", () => "Hello World!");

// Cover service
var coverService = new CoverService(detailsJson);

/// ðŸ”¹ Get all covers / manga details
app.MapGet("/covers", () =>
{
    return coverService.GetCovers();
});

/// ðŸ”¹ Get manga names only
app.MapGet("/names", () =>
{
    return coverService.GetCovers().Select(c => c.Name);
});

/// ðŸ”¹ Get names + URLs (for React)
app.MapGet("/names-url", () =>
{
    return coverService.GetCovers().Select(c => new
    {
        c.Name,
        c.Url
    });
});

/// ðŸ”¹ Get list of pages for a specific manga chapter
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
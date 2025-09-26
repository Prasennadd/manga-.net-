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

// 🔹 Use relative paths (Render safe)
string basePath = Path.Combine(builder.Environment.ContentRootPath, "Mangafiles");
string coversPath = Path.Combine(basePath, "Covers");
string allMangaPath = Path.Combine(basePath, "allmanga");
string detailsJson = Path.Combine(basePath, "details.json");

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

// ✅ Endpoints
app.MapGet("/covers", () => coverService.GetCovers());

app.MapGet("/names", () => coverService.GetCovers().Select(c => c.Name));

app.MapGet("/names-url", () => coverService.GetCovers().Select(c => new { c.Name, c.Url }));

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

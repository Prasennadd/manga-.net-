using System.IO;
using System.Collections.Generic;

namespace webapp.src
{
    public class MangaService
    {
        private readonly string _allMangaPath;

        public MangaService(string allMangaPath)
        {
            _allMangaPath = allMangaPath;
        }

        // Get all manga folder names
        public List<string> GetMangaFolders()
        {
            var folders = new List<string>();

            if (!Directory.Exists(_allMangaPath))
                return folders;

            foreach (var dir in Directory.GetDirectories(_allMangaPath))
            {
                folders.Add(Path.GetFileName(dir));
            }

            return folders;
        }

        // Get all chapter PDF files for a specific manga
        public List<string> GetChapters(string mangaName)
        {
            var chapters = new List<string>();
            var mangaFolder = Path.Combine(_allMangaPath, mangaName);

            if (!Directory.Exists(mangaFolder))
                return chapters;

            foreach (var file in Directory.GetFiles(mangaFolder, "*.pdf"))
            {
                chapters.Add(Path.GetFileName(file));
            }

            return chapters;
        }
    }
}

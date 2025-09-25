using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace webapp.src
{
    public class CoverService
    {
        private readonly string _jsonPath;

        public CoverService(string jsonPath)
        {
            _jsonPath = jsonPath;
        }

        public List<Cover> GetCovers()
        {
            if (!File.Exists(_jsonPath))
                return new List<Cover>();

            string jsonString = File.ReadAllText(_jsonPath);
            var covers = JsonSerializer.Deserialize<List<Cover>>(jsonString);

            return covers ?? new List<Cover>();
        }
    }
}
